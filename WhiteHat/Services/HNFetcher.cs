﻿using System.Net.Http.Json;
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
                case HnStories.Ask:
                    uri = Constants.Ask;
                    break;
                case HnStories.Job:
                    uri = Constants.Job;
                    break;
                case HnStories.Show:
                    uri = Constants.Show;
                    break;
            }

            List<long> result = new List<long>();
            result = await _http.GetFromJsonAsync<List<long>>(uri);

            return result;
        }
        public async Task<HnItemAlgolia> FetchItemAlgolia(long id)
        {
            string uri = string.Format(Constants.ItemAlgolia, id);
            try
            {
                return await _http.GetFromJsonAsync<HnItemAlgolia>(uri);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<HnItem> FetchItem(long id)
        {
            string uri = string.Format(Constants.Item, id);
            return await _http.GetFromJsonAsync<HnItem>(uri);
        }

        public async Task<HnItem> GetKids(long itemId)
        {
            var item = await FetchItem(itemId);

            if (item is null)
            {
                Console.WriteLine("failed to fetch: " + itemId);
                return null;
            }

            if (item.Kids is null || !item.Kids.Any())
            {
                return item;
            }

            foreach (var kid in item.Kids)
            {
                var res = await GetKids(kid);
                if (!res.IsDeleted)
                {
                    item.KidsFetched.Add(res);
                }
            }

            return item;
        }

        public async Task<QueryResult> Query(string q, int page, bool orderByDate = false, DateTimeOffset? since = null, DateTimeOffset? before = null)
        {
            string url = orderByDate ? Constants.QRecent : Constants.QBest;
            url += string.Format(Constants.QPage, page);

            if (!string.IsNullOrWhiteSpace(q))
            {
                url += string.Format(Constants.QQuery, q);
            }

            if (since.HasValue && before.HasValue)
            {
                url += string.Format(Constants.QBetween, since.Value.ToUnixTimeSeconds(), before.Value.ToUnixTimeSeconds());
            }
            else if (since.HasValue)
            {
                url += string.Format(Constants.QSince, since.Value.ToUnixTimeSeconds());
            }
            else if (before.HasValue)
            {
                url += string.Format(Constants.QBefore, before.Value.ToUnixTimeSeconds());
            }

            try
            {
                return await _http.GetFromJsonAsync<QueryResult>(url);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public enum HnStories
    {
        Best,
        Top,
        New,
        Ask,
        Show,
        Job,
    }
}
