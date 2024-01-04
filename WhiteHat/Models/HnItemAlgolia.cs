using System.Text.Json.Serialization;

namespace WhiteHat.Models
{
    public class HnItemAlgolia
    {
        private Uri _url;
        public int Index { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime Created { get; set; }

        [JsonPropertyName("created_at_i")]
        public long CreatedUnix { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("url")]
        public Uri Url 
        {
            get
            {
                return _url;
            }
            set
            {
                if(value == _url)
                {
                    return;
                }
                _url = value;
                EmbedType = _url is null ? EmbedType.NoUrl : EmbedTypeFromUrl(_url.ToString());
                EmbedUrl = EmbedType == EmbedType.NoUrl ? string.Empty : UrlEmbedFromEmbedType(EmbedType, _url.ToString());
            }
        }

        public string EmbedUrl { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("points")]
        public int? Points { get; set; }

        [JsonPropertyName("parent_id")]
        public long? ParentId { get; set; }

        [JsonPropertyName("story_id")]
        public long? StoryId { get; set; }

        [JsonPropertyName("children")]
        public HnItemAlgolia[] Children { get; set; }

        [JsonPropertyName("options")]
        public object[] Options { get; set; }

        public Hit Hit { get; set; }

        public int KidCount { get; set; } = -1;

        public bool ShowComments { get; set; }

        public bool ShowExpand { get; set; }

        public EmbedType EmbedType { get; set; }

        public string TimeSince()
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(CreatedUnix).ToLocalTime();

            TimeSpan timeSpan = DateTime.Now.Subtract(dateTime);

            return timeSpan.TotalSeconds switch
            {
                <= 60 => $"{timeSpan.Seconds} second{(timeSpan.Seconds > 1 ? "s" : "")} ago",

                _ => timeSpan.TotalMinutes switch
                {
                    <= 1 => "a minute ago",
                    < 60 => $"{timeSpan.Minutes} min",
                    _ => timeSpan.TotalHours switch
                    {
                        <= 1 => "an hour ago",
                        < 24 => $"{timeSpan.Hours}h",
                        _ => timeSpan.TotalDays switch
                        {
                            <= 1 => "yesterday",
                            <= 30 => $"{timeSpan.Days}d",

                            <= 60 => "a month ago",
                            < 365 => $"{timeSpan.Days / 30} month{(timeSpan.Days / 30 > 1 ? "s" : "")} ago",

                            <= 365 * 2 => "a year ago",
                            _ => $"{timeSpan.Days / 365}y ago"
                        }
                    }
                }
            };
        }

        public int CountKids()
        {
            int count = 1;
            foreach (var kid in Children)
            {
                if (!kid.Children.Any())
                {
                    if (!string.IsNullOrEmpty(kid.Text) && !string.IsNullOrEmpty(kid.Author))
                    {
                        count++;
                    }
                }
                else
                {
                    count += kid.CountKids();
                }
            }
            KidCount = count;
            return count;
        }

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

    public enum EmbedType
    {
        NoUrl = 0,
        Url = 1,
        YouTube = 2,
        Twitter = 3,
        Reddit = 4,
        Arxiv = 5,
    }
}
