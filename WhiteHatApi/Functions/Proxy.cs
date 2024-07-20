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
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            string restOfPath)
        {
            HttpResponseMessage response;
            string targetUrl = WebUtility.UrlDecode(restOfPath);
            string htmlContent = "<h1>That didn't work...<h1/>";

            // WhiteList requests
            //if (!Constants.ProxyList.Any(domain => targetUrl.Contains(domain, StringComparison.InvariantCultureIgnoreCase)))
            //{
            //    return new ContentResult()
            //    {
            //        Content = htmlContent + $"<p><a href='{targetUrl}'>{targetUrl}</a> is not whitelisted</p>",
            //        ContentType = "text/html",
            //        StatusCode = 403
            //    };
            //}

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
                _logger.LogError(ex.Message, ex);
                return new ContentResult()
                {
                    Content = htmlContent,
                    ContentType = "text/html",
                    StatusCode = (int)HttpStatusCode.BadGateway
                };
            }
            catch (Exception ex)
            {
                // Unexpected error (e.g., HTML parsing issues)
                _logger.LogError(ex.Message, ex);
                return new ContentResult()
                {
                    Content = htmlContent,
                    ContentType = "text/html",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            return new ContentResult()
            {
                Content = htmlContent,
                ContentType = "text/html",
                StatusCode = (int)response.StatusCode
            };
        }

        // Helper function to create consistent error responses 
        private IActionResult CreateErrorResponse(HttpStatusCode statusCode, string message)
        {
            return new ContentResult()
            {
                Content = message,
                ContentType = "text/plain",
                StatusCode = (int)statusCode
            };
        }

        private string FixRelativePaths(string htmlContent, string targetUrl)
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
                _logger.LogError(targetUrl);
            }

            // Handle 'href' attributes
            var hrefNodes = doc.DocumentNode.SelectNodes("//*[@href]");
            if (hrefNodes != null)
            {
                foreach (var link in hrefNodes)
                {
                    FixAttribute(link, "href", baseUrl);
                }
            }

            // Handle 'src' attributes
            var srcNodes = doc.DocumentNode.SelectNodes("//*[@src]");
            if (srcNodes != null)
            {
                foreach (var elem in srcNodes)
                {
                    FixAttribute(elem, "src", baseUrl);
                }
            }

            // Handle 'src' attributes
            var srcsetNodes = doc.DocumentNode.SelectNodes("//*[@srcset]");
            if (srcsetNodes != null)
            {
                foreach (var elem in srcsetNodes)
                {
                    FixAttribute(elem, "srcset", baseUrl);
                }
            }

            return doc.DocumentNode.OuterHtml;
        }

        private void FixAttribute(HtmlNode node, string attributeName, string baseUrl)
        {
            var currentAttributeValue = node.GetAttributeValue(attributeName, "");
            node.SetAttributeValue(attributeName, ConvertToAbsoluteUrl(currentAttributeValue, baseUrl));
        }

        private string AdjustLinksForNewTab(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            if (doc != null && doc.DocumentNode != null && doc.DocumentNode.ChildNodes != null && doc.DocumentNode.ChildNodes.Any())
            {
                var links = doc.DocumentNode.SelectNodes("//a");
                if (links != null)
                {
                    foreach (var link in links)
                    {
                        link.SetAttributeValue("target", "_blank");
                    }
                }
            }

            return doc.DocumentNode.OuterHtml;
        }

        private string ConvertToAbsoluteUrl(string relativeUrl, string baseUrl)
        {
            string result = relativeUrl;
            try
            {
                if (!string.IsNullOrEmpty(relativeUrl) && !relativeUrl.StartsWith("http") && !relativeUrl.StartsWith("localhost") && !relativeUrl.StartsWith("data") && !relativeUrl.StartsWith("#"))
                {
                    result = "https://" + baseUrl + (relativeUrl.StartsWith("/") ? string.Empty : "/") + relativeUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RelativeUrl: {relativeUrl}, BaseUri: {baseUrl} failed with:");
                _logger.LogError(ex.Message, ex);
            }
             
            return result;
        }
    }
}
