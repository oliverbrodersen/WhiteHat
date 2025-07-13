using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

public class Proxy
{
    private readonly ILogger<Proxy> _logger;
    private readonly HttpClient _httpClient;
    private readonly IDistributedCache _cache;

    // Inject IHttpClientFactory (via HttpClient) and IDistributedCache
    public Proxy(ILogger<Proxy> logger, IHttpClientFactory httpClientFactory, IDistributedCache cache)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _cache = cache;
    }

    [Function("Proxy")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
        string restOfPath)
    {
        if (string.IsNullOrWhiteSpace(restOfPath))
        {
            return new BadRequestObjectResult("Target URL is missing.");
        }

        string targetUrl = WebUtility.UrlDecode(restOfPath);
        string cacheKey = $"proxy_v1_{targetUrl}"; // Added a version to the key

        // 1. Try to get the content from cache
        try
        {
            byte[] cachedContent = await _cache.GetAsync(cacheKey);
            if (cachedContent != null)
            {
                _logger.LogInformation("Cache hit for URL: {TargetUrl}", targetUrl);
                return new ContentResult()
                {
                    Content = Encoding.UTF8.GetString(cachedContent),
                    ContentType = "text/html; charset=utf-8",
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis cache read error for URL: {TargetUrl}", targetUrl);
            // If cache fails, proceed to fetch from source instead of failing the request
        }

        _logger.LogInformation("Cache miss for URL: {TargetUrl}", targetUrl);

        // 2. Fetch from the target URL if not in cache
        HttpResponseMessage response;
        string htmlContent;
        try
        {
            // Use the injected HttpClient
            response = await _httpClient.GetAsync(targetUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Proxy target returned unsuccessful status code: {StatusCode} for URL: {TargetUrl}", response.StatusCode, targetUrl);
                return new ContentResult()
                {
                    Content = $"<h1>Error: The requested site returned a {response.StatusCode} status.</h1>",
                    ContentType = "text/html; charset=utf-8",
                    StatusCode = (int)response.StatusCode
                };
            }

            htmlContent = await response.Content.ReadAsStringAsync();

            // 3. Modify HTML
            htmlContent = FixRelativePaths(htmlContent, targetUrl);
            htmlContent = AdjustLinksForNewTab(htmlContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error during web request to proxied site: {TargetUrl}", targetUrl);
            return CreateErrorResponse(HttpStatusCode.BadGateway, "<h1>That didn't work... The remote server is not responding.</h1>");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing URL: {TargetUrl}", targetUrl);
            return CreateErrorResponse(HttpStatusCode.InternalServerError, "<h1>An unexpected error occurred.</h1>");
        }

        // 4. Store the processed content in cache
        try
        {
            var cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30)); // Cache for 30 minutes of inactivity

            byte[] contentToCache = Encoding.UTF8.GetBytes(htmlContent);
            await _cache.SetAsync(cacheKey, contentToCache, cacheEntryOptions);
            _logger.LogInformation("Successfully cached content for URL: {TargetUrl}", targetUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis cache write error for URL: {TargetUrl}", targetUrl);
        }

        // 5. Return the result
        return new ContentResult()
        {
            Content = htmlContent,
            ContentType = "text/html; charset=utf-8",
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    private string FixRelativePaths(string htmlContent, string targetUrl)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // A more reliable way to get the base for resolving relative URLs
        var baseUri = new Uri(targetUrl);

        // Select all relevant nodes at once
        var nodes = doc.DocumentNode.SelectNodes("//*[@href or @src]");
        if (nodes == null) return doc.DocumentNode.OuterHtml;

        foreach (var node in nodes)
        {
            FixAttribute(node, "href", baseUri);
            FixAttribute(node, "src", baseUri);
        }

        // Special handling for srcset which can contain multiple URLs
        var srcsetNodes = doc.DocumentNode.SelectNodes("//*[@srcset]");
        if (srcsetNodes != null)
        {
            foreach (var node in srcsetNodes)
            {
                var originalSrcset = node.GetAttributeValue("srcset", "");
                if (string.IsNullOrWhiteSpace(originalSrcset)) continue;

                var newSrcsetParts = originalSrcset.Split(',')
                    .Select(part => part.Trim().Split(' '))
                    .Select(parts =>
                    {
                        if (parts.Length > 0 && Uri.TryCreate(baseUri, parts[0], out var absoluteUri))
                        {
                            parts[0] = absoluteUri.ToString();
                        }
                        return string.Join(" ", parts);
                    });

                node.SetAttributeValue("srcset", string.Join(", ", newSrcsetParts));
            }
        }

        return doc.DocumentNode.OuterHtml;
    }

    private void FixAttribute(HtmlNode node, string attributeName, Uri baseUri)
    {
        var attr = node.Attributes[attributeName];
        if (attr == null || string.IsNullOrWhiteSpace(attr.Value)) return;

        // Skip data URIs and anchor links
        if (attr.Value.StartsWith("data:") || attr.Value.StartsWith("#")) return;

        // The Uri constructor reliably handles joining relative and absolute paths
        if (Uri.TryCreate(baseUri, attr.Value, out Uri? absoluteUri))
        {
            attr.Value = absoluteUri.ToString();
        }
    }

    private string AdjustLinksForNewTab(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        var links = doc.DocumentNode.SelectNodes("//a[@href]");
        if (links != null)
        {
            foreach (var link in links)
            {
                link.SetAttributeValue("target", "_blank");
                // Add rel="noopener noreferrer" for security when using target="_blank"
                link.SetAttributeValue("rel", "noopener noreferrer");
            }
        }

        return doc.DocumentNode.OuterHtml;
    }

    private IActionResult CreateErrorResponse(HttpStatusCode statusCode, string message)
    {
        return new ContentResult()
        {
            Content = message,
            ContentType = "text/html; charset=utf-8",
            StatusCode = (int)statusCode
        };
    }
}