using System.Text.Json.Serialization;
using WhiteHat.Enums;
using WhiteHat.Misc;

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
                EmbedType = _url is null ? EmbedType.NoUrl : EmbedHelper.EmbedTypeFromUrl(_url.ToString());
                EmbedUrl = EmbedType == EmbedType.NoUrl ? string.Empty : EmbedHelper.UrlEmbedFromEmbedType(EmbedType, _url.ToString());
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

        public int CountKids(bool isFirst = false)
        {
            int count = isFirst ? 0 : 1;
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
    }
}
