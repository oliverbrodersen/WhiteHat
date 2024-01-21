using Microsoft.AspNetCore.Components;
using WhiteHat.Models;
using WhiteHat.Services;

namespace WhiteHat.Pages
{
    public partial class User
    {
        [Parameter]
        public string? UserName { get; set; }

        private QueryResult _result;
        private int _page = 0;
        private int _loadedStories = 0;
        private bool _isLoading, _settingsOpen, _orderByDate, _noMoreToLoad;
        List<HnItemAlgolia> _items;
        private HnUser user;

        protected async override Task OnParametersSetAsync()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                NavigationManager.NavigateTo("/");
            }
            user = await HNFetcher.GetUser(UserName);
            if (user is null)
            {
                NavigationManager.NavigateTo("/");
            }
            await LoadFromFresh();
        }

        private async Task LoadFromFresh()
        {
            _items = new();
            _page = 0;
            _loadedStories = 0;
            await LoadStories();
        }

        private async Task LoadStories()
        {
            _isLoading = true;
            StateHasChanged();

            _result = await HNFetcher.GetUserPosts(UserName,
                                            _page++,
                                            _orderByDate);

            await LoadStory(_result.Hits.Length - 1);

            if (_result.NbHits == _items.Count())
            {
                _noMoreToLoad = true;
            }

            StateHasChanged();
        }

        private async Task LoadStory(int _leftToLoad)
        {
            int i = _result.Hits.Length - 1 - _leftToLoad;

            if (_result.Hits.Count() <= i)
            {
                _noMoreToLoad = true;
            }
            else if (!_items.Select(x => x.Id).Contains(_result.Hits[i].Id))
            {
                var item = await HNFetcher.FetchItemAlgolia(_result.Hits[i].Id);

                if (item is not null)
                {
                    item.Hit = _result.Hits[i];
                    item.Index = ++_loadedStories;
                    _items.Add(item);
                    StateHasChanged();
                }
            }

            if(_noMoreToLoad || _leftToLoad <= 0)
                _isLoading = false;
            else 
                await LoadStory(--_leftToLoad);
        }

        private async Task SwitchSort()
        {
            _orderByDate = !_orderByDate;
            await LoadFromFresh();
        }
    }
}