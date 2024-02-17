using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Security.Cryptography;
using WhiteHat.Enums;
using WhiteHat.Misc;
using WhiteHat.Models;
using WhiteHat.Services;

namespace WhiteHat.Pages
{
    public partial class Index
    {
        [Parameter]
        public string? Page { get; set; }

        private readonly int _pageSize = Constants.PageSize;
        private int _loadedStories = 0;
        private bool _isLoading;
        List<HnItemAlgolia> _items;
        private HnItemAlgolia _shownItem;
        List<long> _itemIds;
        HnStories story;

        protected override async Task OnParametersSetAsync()
        {
            if (string.IsNullOrEmpty(Page))
            {
                story = HnStories.Best;
            }
            else
            {
                switch (Page.ToLower())
                {
                    case "best":
                        story = HnStories.Best;
                        break;
                    case "new":
                        story = HnStories.New;
                        break;
                    case "top":
                        story = HnStories.Top;
                        break;
                    case "ask":
                        story = HnStories.Ask;
                        break;
                    case "show":
                        story = HnStories.Show;
                        break;
                    case "job":
                        story = HnStories.Job;
                        break;
                    default:
                        story = HnStories.Top;
                        break;
                }
            }
            _items = new();
            _itemIds = new();
            _loadedStories = 0;

            //HnItemAlgolia item = new();
            //item.Author = "WhiteHat";
            //item.Created = DateTime.Now.AddHours(-2);
            //DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //item.CreatedUnix = (long)(item.Created - unixEpoch).TotalSeconds;
            //item.KidCount = 1124;
            //item.Points = 789;
            //item.Children = [];
            //item.Index = ++_loadedStories;
            //item.Title = "White Hat - A modern, single page, hacker news UI";
            //item.Url = new Uri("https://whitehat.oliver-brodersen.com");
            //_items.Add(item);

            await LoadStories();
        }

        private async Task LoadStories()
        {
            HnStories stloaded = story;
            _isLoading = true;
            StateHasChanged();

            if (_itemIds is null)
            {
                _itemIds = await HNFetcher.FetchStories(stloaded);
            }
            else if (_itemIds.Count() <= _loadedStories)
            {
                _itemIds.AddRange(await HNFetcher.FetchStories(stloaded));
            }
            
            await LoadStory(_pageSize);

            StateHasChanged();
        }

        private async Task LoadStory(int _leftToLoad)
        {
            if (_leftToLoad <= 0  || _loadedStories > _itemIds.Count() - 1)
            {
                _isLoading = false;
                return;
            }

            if (!_items.Select(x => x.Id).Contains(_itemIds[_loadedStories]))
            {
                var item = await HNFetcher.FetchItemAlgolia(_itemIds[_loadedStories]);

                if (item is not null)
                {
                    item.Index = ++_loadedStories;
                    _items.Add(item);
                    StateHasChanged();
                }
            }

            if (_leftToLoad > 1)
                await LoadStory(--_leftToLoad);
            else
                _isLoading = false;
        }
    }
}