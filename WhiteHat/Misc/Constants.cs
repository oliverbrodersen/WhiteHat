namespace WhiteHat.Misc
{
    public static class Constants
    {
        public static readonly string HnApiBase = "https://hacker-news.firebaseio.com/v0/";
        public static readonly string Top = HnApiBase + "topstories.json";
        public static readonly string Best = HnApiBase + "beststories.json";
        public static readonly string New = HnApiBase + "newstories.json";

        public static readonly string Item = HnApiBase + "/item/{0}.json";
        public static readonly string ItemAlgolia = "https://hn.algolia.com/api/v1/items/{0}";
    }
}
