namespace WhiteHat.Models
{
    using System;
    using System.Text.Json.Serialization;

    public class HnItem
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("deleted")]
        public bool IsDeleted { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("by")]
        public string By { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("dead")]
        public bool IsDead { get; set; }

        [JsonPropertyName("parent")]
        public long Parent { get; set; }

        [JsonPropertyName("poll")]
        public long Poll { get; set; }

        [JsonPropertyName("kids")]
        public long[] Kids { get; set; }

        public int KidCount { get; set; }

        public bool KidsCounted { get; set; }

        public List<HnItem> KidsFetched { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("score")]
        public long Score { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("parts")]
        public long[] Parts { get; set; }

        [JsonPropertyName("descendants")]
        public long Descendants { get; set; }


        public HnItem()
        {
            KidsFetched = new();
        }

        public string TimeSince()
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(Time).ToLocalTime();

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
    }
}
