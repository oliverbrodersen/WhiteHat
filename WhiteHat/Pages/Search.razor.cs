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
        private bool _isLoading,_settingsOpen, _noMoreToLoad, _onlyMatchTitle = true;
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

            if (_result is null || _result.Hits is null || !_result.Hits.Any())
            {
                _noMoreToLoad = true;
                _isLoading = false;
                StateHasChanged();
                return;

            }
            await LoadStory(_result.Hits.Length);

            if (_result.NbHits == _items.Count())
            {
                _noMoreToLoad = true;
            }

            StateHasChanged();
        }

        private async Task LoadStory(int _leftToLoad)
        {
            int i = _result.Hits.Length - _leftToLoad;

            if (_result.Hits.Length <= i)
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

            if (_noMoreToLoad || _leftToLoad <= 0)
                _isLoading = false;
            else
                await LoadStory(--_leftToLoad);
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