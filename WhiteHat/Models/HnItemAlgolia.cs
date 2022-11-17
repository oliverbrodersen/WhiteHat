using System.Text.Json.Serialization;

namespace WhiteHat.Models
{
    public class HnItemAlgolia
    {
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
        public Uri Url { get; set; }

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
                    < 60 => $"{timeSpan.Minutes} minute{(timeSpan.Seconds > 1 ? "s" : "")} ago",
                    _ => timeSpan.TotalHours switch
                    {
                        <= 1 => "an hour ago",
                        < 24 => $"{timeSpan.Hours} hour{(timeSpan.Hours > 1 ? "s" : "")} ago",
                        _ => timeSpan.TotalDays switch
                        {
                            <= 1 => "yesterday",
                            <= 30 => $"{timeSpan.Days} day{(timeSpan.Days > 1 ? "s" : "")} ago",

                            <= 60 => "a month ago",
                            < 365 => $"{timeSpan.Days / 30} month{(timeSpan.Days / 30 > 1 ? "s" : "")} ago",

                            <= 365 * 2 => "a year ago",
                            _ => $"{timeSpan.Days / 365} years ago"
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
    }
}
