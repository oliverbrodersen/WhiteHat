using System.Text.Json.Serialization;

namespace WhiteHat.Models
{
    public class HnUser
    {
        [JsonPropertyName("about")]
        public string About { get; set; }

        [JsonPropertyName("karma")]
        public long Karma { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
}
