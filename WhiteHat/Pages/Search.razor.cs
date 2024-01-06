using Microsoft.AspNetCore.Components;
using WhiteHat.Enums;
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
        private int _page = 0;
        private int _loadedStories = 0;
        private bool _isLoading,_settingsOpen, _onlyMatchTitle = true;
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
            _loadedStories = 0;
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

            await LoadStory(_result.Hits.Length - 1);

            StateHasChanged();
        }

        private async Task LoadStory(int _leftToLoad)
        {
            if (!_items.Select(x => x.Id).Contains(_result.Hits[_result.Hits.Length - 1 - _leftToLoad].Id))
            {
                var item = await HNFetcher.FetchItemAlgolia(_result.Hits[_result.Hits.Length - 1 - _leftToLoad].Id);

                if (item is not null)
                {
                    item.Hit = _result.Hits[_result.Hits.Length - 1 - _leftToLoad];
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
    }
}