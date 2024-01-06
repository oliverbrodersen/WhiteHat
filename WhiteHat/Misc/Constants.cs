using Version = WhiteHat.Models.Version;
namespace WhiteHat.Misc
{
    public static class Constants
    {
        public static readonly int PageSize = 25;
        public static readonly string HnApiBase = "https://hacker-news.firebaseio.com/v0/";
        public static readonly string Best = HnApiBase + "topstories.json";
        public static readonly string Top = HnApiBase + "beststories.json";
        public static readonly string New = HnApiBase + "newstories.json";
        public static readonly string Job = HnApiBase + "jobstories.json";
        public static readonly string Ask = HnApiBase + "askstories.json";
        public static readonly string Show = HnApiBase + "showstories.json";

        public static readonly string Item = HnApiBase + "/item/{0}.json";
        public static readonly string ItemAlgolia = "https://hn.algolia.com/api/v1/items/{0}";

        public static readonly string QPagesize = "hitsPerPage=";
        public static readonly string QPage = "&page={0}";
        public static readonly string QOnlyTitle = "&restrictSearchableAttributes=title";
        public static readonly string QStoriesOnly = "&tags=story";
        public static readonly string QBest = "https://hn.algolia.com/api/v1/search?" + QPagesize + PageSize + QStoriesOnly;
        public static readonly string QRecent = "https://hn.algolia.com/api/v1/search_by_date?" + QPagesize + PageSize + QStoriesOnly;
        public static readonly string QQuery = "&query={0}";
        public static string SettingsKeyId = "settings";
        public static readonly string QNumericFilters = "&numericFilters=";
        public static readonly string QCreatedAt = "created_at_i";
        public static readonly string QPoints = "points";
        public static readonly string QNumComments = "num_comments";
        public static readonly string QSince = "{0}>{1}";
        public static readonly string QBefore = "{0}<{1}";

        public static string VersionKeyId = "version";
        public static Version CurrentVersion = new Version(0, 3, 9);
    }
}
