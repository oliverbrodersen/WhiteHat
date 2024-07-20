namespace WhiteHat.Services
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public class IframeCheckerService
    {
        private readonly HttpClient _httpClient;

        public IframeCheckerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> CanDisplayInIframe(string url)
        {
            try
            {
                var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

                if (response.Headers.Contains("X-Frame-Options"))
                {
                    var xFrameOptions = response.Headers.GetValues("X-Frame-Options").FirstOrDefault();
                    if (xFrameOptions == "DENY" || xFrameOptions == "SAMEORIGIN")
                    {
                        return false;
                    }
                }

                if (response.Headers.Contains("Content-Security-Policy"))
                {
                    var csp = response.Headers.GetValues("Content-Security-Policy").FirstOrDefault();
                    if (csp.Contains("frame-ancestors"))
                    {
                        // You need to parse the CSP header value to check if your domain is allowed
                        var frameAncestors = csp.Split(';')
                                                .FirstOrDefault(d => d.Trim().StartsWith("frame-ancestors"));

                        if (frameAncestors != null && !frameAncestors.Contains("your-domain.com"))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("TypeError: Failed to fetch"))
                {
                    return false;
                }
                return false;
            }
        }
    }

}
