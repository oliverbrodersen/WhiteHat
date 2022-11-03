using System.Net.Http.Json;
using WhiteHat.Misc;
using WhiteHat.Models;

namespace WhiteHat.Services
{
    public class HNFetcher
    {
        public HNFetcher(HttpClient httpClient)
        {
            _http = httpClient;
        }
        private readonly HttpClient _http;

        public async Task<List<long>> FetchStories(HnStories story)
        {
            string uri = string.Empty;

            switch (story)
            {
                case HnStories.Best:
                    uri = Constants.Best;
                    break;
                case HnStories.Top:
                    uri = Constants.Top;
                    break;
                case HnStories.New:
                    uri = Constants.New;
                    break;
            }

            List<long> result = new List<long>();
            result = await _http.GetFromJsonAsync<List<long>>(uri);

            return result;
        }

        public async Task<HnItem> FetchItem(long id)
        {
            string uri = string.Format(Constants.Item, id);
            return await _http.GetFromJsonAsync<HnItem>(uri);
        }
    }

    public enum HnStories
    {
        Best,
        Top,
        New,
    }
}
