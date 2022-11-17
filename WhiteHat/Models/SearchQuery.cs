using System.Text.Json.Serialization;
namespace WhiteHat.Models
{

    public partial class QueryResult
    {
        [JsonPropertyName("hits")]
        public Hit[] Hits { get; set; }

        [JsonPropertyName("page")]
        public long Page { get; set; }

        [JsonPropertyName("nbHits")]
        public long NbHits { get; set; }

        [JsonPropertyName("nbPages")]
        public long NbPages { get; set; }

        [JsonPropertyName("hitsPerPage")]
        public long HitsPerPage { get; set; }

        [JsonPropertyName("processingTimeMS")]
        public long ProcessingTimeMs { get; set; }

        [JsonPropertyName("query")]
        public string Query { get; set; }

        [JsonPropertyName("params")]
        public string Params { get; set; }
    }

    public partial class Hit
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("points")]
        public long? Points { get; set; }

        [JsonPropertyName("created_at_i")]
        public long CreateAtI { get; set; }

        [JsonPropertyName("story_text")]
        public string StoryText { get; set; }

        [JsonPropertyName("comment_text")]
        public string CommentText { get; set; }

        [JsonPropertyName("_tags")]
        public string[] Tags { get; set; }

        [JsonPropertyName("num_comments")]
        public long? NumComments { get; set; }

        [JsonPropertyName("objectID")]
        public string ObjectId { get; set; }

        [JsonPropertyName("_highlightResult")]
        public HighlightResult HighlightResult { get; set; }

        public long Id
        {
            get
            {
                return long.Parse(ObjectId);
            }
        }
    }

    public partial class HighlightResult
    {
        [JsonPropertyName("title")]
        public Highlight Title { get; set; }

        [JsonPropertyName("url")]
        public Highlight Url { get; set; }

        [JsonPropertyName("author")]
        public Highlight Author { get; set; }
    }

    public partial class Highlight
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("matchLevel")]
        public string MatchLevel { get; set; }

        [JsonPropertyName("matchedWords")]
        public string[] MatchedWords { get; set; }
    }
}
