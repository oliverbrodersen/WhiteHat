using Azure;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using WhiteHat.Enums;
using WhiteHat.Misc;

namespace WhiteHatApi.Functions
{
    public class Proxy
    {
        private readonly ILogger<Proxy> _logger;

        public Proxy(ILogger<Proxy> logger)
        {
            _logger = logger;
        }

        [Function("Proxy")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "proxy/{*restOfPath}")] HttpRequest req,
            string restOfPath)
        {
            HttpResponseMessage response;
            string targetUrl = WebUtility.UrlDecode(restOfPath);
            string htmlContent = "<h1>That didn't work...<h1/>";

            if (!Constants.ProxyList.Any(domain => targetUrl.Contains(domain, StringComparison.InvariantCultureIgnoreCase)))
            {
                return new ContentResult()
                {
                    Content = htmlContent + $"<p><a href='{targetUrl}'>{targetUrl}</a> is not whitelisted</p>",
                    ContentType = "text/html",
                    StatusCode = 403
                };
            }

            try
            {

                using (var httpClient = new HttpClient())
                {
                    response = await httpClient.GetAsync(targetUrl);

                    // Add CORS header
                    response.Headers.Add("Access-Control-Allow-Origin", "*");

                    htmlContent = await response.Content.ReadAsStringAsync();

                    // HTML Modifications
                    htmlContent = FixRelativePaths(htmlContent, targetUrl);
                    htmlContent = AdjustLinksForNewTab(htmlContent);
                }
            }
            catch (HttpRequestException ex)
            {
                // Error during the web request to the proxied site
                return CreateErrorResponse(HttpStatusCode.BadGateway, "Error fetching from target website.");
            }
            catch (Exception ex)
            {
                // Unexpected error (e.g., HTML parsing issues)
                return CreateErrorResponse(HttpStatusCode.InternalServerError, "An internal error occurred.");
            }

            return new ContentResult()
            {
                Content = htmlContent,
                ContentType = "text/html",
                StatusCode = (int)response.StatusCode
            };
        }

        // Helper function to create consistent error responses 
        private static IActionResult CreateErrorResponse(HttpStatusCode statusCode, string message)
        {
            return new ContentResult()
            {
                Content = message,
                ContentType = "text/plain",
                StatusCode = (int)statusCode
            };
        }

        private static string FixRelativePaths(string htmlContent, string targetUrl)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            bool hasHost = Uri.TryCreate(targetUrl,UriKind.Absolute, out Uri? baseUri);
            string baseUrl = targetUrl;
            if (baseUri != null)
            {
                baseUrl = baseUri.Host.ToString();
            }
            else
            {
                Console.WriteLine(targetUrl);
            }

            if (doc.DocumentNode is not null && doc.DocumentNode.Depth > 0)
            {
                // Handle 'href' attributes 
                foreach (var link in doc.DocumentNode.SelectNodes("//*[@href]"))
                {
                    FixAttribute(link, "href", baseUrl);
                }
            }

            if (doc.DocumentNode is not null && doc.DocumentNode.Depth > 0)
            {
                // Handle 'src' attributes
                foreach (var elem in doc.DocumentNode.SelectNodes("//*[@src]"))
                {
                    FixAttribute(elem, "src", baseUrl);
                }
            }

            // You can add handling for more attributes as needed

            return doc.DocumentNode.OuterHtml;
        }

        private static void FixAttribute(HtmlNode node, string attributeName, string baseUrl)
        {
            var currentAttributeValue = node.GetAttributeValue(attributeName, "");
            node.SetAttributeValue(attributeName, ConvertToAbsoluteUrl(currentAttributeValue, baseUrl));
        }

        private static string AdjustLinksForNewTab(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            foreach (var link in doc.DocumentNode.SelectNodes("//a"))
            {
                link.SetAttributeValue("target", "_blank");
            }

            return doc.DocumentNode.OuterHtml;
        }

        private static string ConvertToAbsoluteUrl(string relativeUrl, string baseUrl)
        {
            string result = relativeUrl;
            try
            {
                if (!string.IsNullOrEmpty(relativeUrl) && !relativeUrl.StartsWith("http") && !relativeUrl.StartsWith("#"))
                {
                    result = baseUrl + (relativeUrl.StartsWith("/") ? string.Empty : "/") + relativeUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RelativeUrl: {relativeUrl}, BaseUri: {baseUrl} failed with:");
                Console.Error.WriteLine(ex);
            }

            Console.WriteLine(result);
            return result;
        }
    }
}
