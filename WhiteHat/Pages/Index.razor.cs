using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Security.Cryptography;
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
        private int _selectedIndex = -1;
        private bool _isLoading, _stop, _showSmallPreview, _isIframeLoading, _isFrameOpen, _isComments;
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
            _stop = _isLoading;
            await LoadStories();
        }


        protected async override Task OnInitializedAsync()
        {
            await JSRuntime.InvokeVoidAsync("setupKeyboardShortcuts", DotNetObjectReference.Create(this));
        }

        private async Task LoadStories()
        {
            HnStories stloaded = story;
            _isLoading = true;
            StateHasChanged();
            _itemIds = await HNFetcher.FetchStories(stloaded);

            await LoadStory(_pageSize);

            _isLoading = _stop;
            _stop = false;
            StateHasChanged();
        }

        private async Task LoadStory(int _leftToLoad)
        {
            var item = await HNFetcher.FetchItemAlgolia(_itemIds[_loadedStories]);

            if (!_stop)
            {
                if (item is not null)
                {
                    item.Index = ++_loadedStories;
                    _items.Add(item);
                    StateHasChanged();
                }
            }
            else
                return;

            if (_leftToLoad > 1)
                await LoadStory(--_leftToLoad);
        }

        private void OpenItem(HnItemAlgolia item, int i)
        {
            if (_shownItem == item)
            {
                _shownItem = null;
                _isFrameOpen = false;
            }
            else
            {
                _isIframeLoading = true;
                _shownItem = item;
                _selectedIndex = i;
            }
        }

        private void CloseLinkPane()
        {
            _shownItem = null;
        }

        private async Task OpenInNewTab()
        {
            await JSRuntime.InvokeVoidAsync("open", _shownItem.Url, "_blank");
        }

        private void TogglePrevSize()
        {
            _showSmallPreview = !_showSmallPreview;
        }

        private void ToggleContent()
        {
            _isComments = !_isComments;
        }

        private async Task ScrollToItem(long Id)
        {
            await JSRuntime.InvokeVoidAsync("scrollToItem", Id);
        }

        private async Task SelectItem(HnItemAlgolia item)
        {
            _selectedIndex = item.Index - 1;
            StateHasChanged();
            await ScrollToItem(item.Id);
        }

        [JSInvokable]
        public async Task HandleUp(bool select = false)
        {
            if (_items is null)
                return;

            if (_selectedIndex > 0)
            {
                _selectedIndex--;
            }
            var item = _items[_selectedIndex];
            await SelectItem(item);

            if (select && item.Url is not null)
            {
                _shownItem = item;
                StateHasChanged();
            }

            await SelectItem(_items[_selectedIndex]);
        }

        [JSInvokable]
        public async Task HandleDown(bool select = false)
        {
            if (_items is null)
                return;

            if (_selectedIndex < _items.Count - 1)
            {
                _selectedIndex++;
            }
            var item = _items[_selectedIndex];
            await SelectItem(item);

            if (select && item.Url is not null)
            {
                _shownItem = item;
                StateHasChanged();
            }

            if (_selectedIndex >= _items.Count - 1 && !_isLoading)
            {
                await LoadStories();
            }
        }

        [JSInvokable]
        public void HandleCommentToggle()
        {
            if (_selectedIndex > -1)
            {
                var item = _items[_selectedIndex];
                item.ShowComments = !item.ShowComments;
                StateHasChanged();
            }
        }

        [JSInvokable]
        public void HandleExpand()
        {
            if (_selectedIndex > -1)
            {
                var item = _items[_selectedIndex];
                item.ShowExpand = !item.ShowExpand;
                StateHasChanged();
            }
        }

        [JSInvokable]
        public void HandlePreviewToggle()
        {
            if (_selectedIndex > -1)
            {
                var item = _items[_selectedIndex];

                if (item is null || item.Url is null)
                    return;

                if (_shownItem == item)
                {
                    _shownItem = null;
                    _isFrameOpen = false;
                }
                else
                {
                    _shownItem = item;
                    _isIframeLoading = true;
                }
                StateHasChanged();
            }
        }

        [JSInvokable]
        public async Task HandleOpen()
        {
            if (_selectedIndex > -1)
            {
                var item = _items[_selectedIndex];

                if (item is null || item.Url is null)
                    return;

                await JSRuntime.InvokeVoidAsync("open", item.Url, "_blank");
            }
        }

        public void IFrameLoaded()
        {
            _isIframeLoading = false;
            _isFrameOpen = true;
            StateHasChanged();
        }


        public void Dispose()
        {
            // Remove the event listener when the component is disposed
            JSRuntime.InvokeVoidAsync("removeKeyboardShortcuts");
        }
    }
}