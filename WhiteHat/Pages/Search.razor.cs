using Microsoft.AspNetCore.Components;
using WhiteHat.Misc;
using WhiteHat.Models;
using WhiteHat.Services;

namespace WhiteHat.Pages
{
    public partial class Search
    {
        [Parameter]
        public string? Query { get; set; }

        private QueryResult _result;
        private readonly int _pageSize = Constants.PageSize;
        private int _page = 0;
        private int _loadedStories = 0;
        private bool _isLoading, _stop, _settingsOpen, _onlyMatchTitle = true;
        List<HnItemAlgolia> _items;
        private bool _orderByDate;

        private DateTimeOffset _toDate, _fromDate;
        private int? _toPoints, _fromPoints, _toComments, _fromComments;

        protected async override Task OnParametersSetAsync()
        {
            await LoadFromFresh();
        }

        protected override void OnInitialized()
        {
            _toDate = DateTime.Today;
            _fromDate = new DateTime(2007, 1, 1);
        }

        private async Task LoadFromFresh()
        {
            _items = new();
            _page = 0;
            _stop = _isLoading;
            await LoadStories();
        }

        private async Task LoadStories()
        {
            _isLoading = true;
            StateHasChanged();
            _result = await HNFetcher.Query(Query,
                                            _page++,
                                            _orderByDate,
                                            _onlyMatchTitle,
                                            _fromDate,
                                            _toDate,
                                            _toPoints,
                                            _fromPoints,
                                            _toComments,
                                            _fromComments);
            _loadedStories = 0;
            if (_result is not null)
            {
                await LoadStory(_result.Hits.Length - 1);
                _isLoading = _stop;
            }
            else
            {
                _isLoading = false;
            }

            _stop = false;
            StateHasChanged();
        }

        private void SwitchSort()
        {
            _orderByDate = !_orderByDate;
        }

        private void SwitchOnlyMatchTitle()
        {
            _onlyMatchTitle = !_onlyMatchTitle;
        }

        private void ToggleSettings()
        {
            _settingsOpen = !_settingsOpen;
            StateHasChanged();
        }

        private async Task LoadStory(int _leftToLoad)
        {
            var item = await HNFetcher.FetchItemAlgolia(_result.Hits[_loadedStories].Id);
            if (!_stop)
            {
                if (item is not null)
                {
                    item.Hit = _result.Hits[_loadedStories];
                    _items.Add(item);
                    _loadedStories++;
                    StateHasChanged();
                }
            }
            else
                return;
            if (_leftToLoad > 1)
                await LoadStory(--_leftToLoad);
        }

        private enum SortTypes
        {
            Popular,
            Time
        }
    }
}