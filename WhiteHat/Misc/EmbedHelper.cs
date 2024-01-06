using WhiteHat.Enums;

namespace WhiteHat.Misc
{
    public static class EmbedHelper
    {
        /// <summary>
        /// Provide a url and get the embed type.
        /// </summary>
        /// <param name="url">Complete url as a string</param>
        /// <returns>Type of embed</returns>
        public static EmbedType EmbedTypeFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                //If its not a url
                return EmbedType.NoUrl;
            }

            //Is it any particular site?
            if (url.Contains("youtube.com/watch?v=", StringComparison.InvariantCultureIgnoreCase))
            {
                return EmbedType.YouTube;
            }
            if (url.Contains("reddit.com/r/", StringComparison.InvariantCultureIgnoreCase))
            {
                return EmbedType.Reddit;
            }
            if (url.Contains("twitter.com", StringComparison.InvariantCultureIgnoreCase))
            {
                return EmbedType.Twitter;
            }
            if (url.Contains("arxiv.org", StringComparison.InvariantCultureIgnoreCase))
            {
                return EmbedType.Arxiv;
            }

            //Just embed the post
            return EmbedType.Url;
        }

        /// <summary>
        /// Converts url to embed url based on EmbedType.
        /// </summary>
        /// <param name="type">The embed type</param>
        /// <param name="url">The complete url to be formatted as a string</param>
        /// <returns>Url to embed as a string</returns>
        public static string UrlEmbedFromEmbedType(EmbedType type, string url)
        {
            string result;
            string[]? splitUrl;

            try
            {
                switch (type)
                {
                    case EmbedType.NoUrl:
                        return string.Empty;
                    case EmbedType.YouTube:
                        return url.Replace("youtube.com/watch?v=", "youtube.com/embed/", StringComparison.InvariantCultureIgnoreCase);
                    case EmbedType.Reddit:
                        splitUrl = url.Split("//")[^1].Split('/');
                        return string.Format("https://embed.reddit.com/r/{0}/comments/{1}/{0}/?embed=true&ref_source=embed&ref=share&utm_medium=widgets&utm_source=embedv2&utm_term=23&theme=dark&utm_name=post_embed&embed_host_url=whitehat.oliver-brodersen.com", splitUrl[2], splitUrl[4]);
                    case EmbedType.Twitter:
                        splitUrl = url.Split("//")[^1].Split('/');
                        return string.Format("https://platform.twitter.com/embed/Tweet.html?embedId=twitter-widget-8&id={0}&lang=en&theme=dark&width=1920px", splitUrl[3]);
                    case EmbedType.Arxiv:
                        splitUrl = url.Split("//")[^1].Split('/');
                        return string.Format("https://arxiv.org/pdf/{0}.pdf", splitUrl[2]);
                    case EmbedType.Url:
                    default:
                        return url;
                }
            }
            catch
            {
                return url;
            }
        }
    }
}
