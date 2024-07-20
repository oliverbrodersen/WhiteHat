using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Runtime;
using WhiteHat.Enums;
using WhiteHat.Misc;
using WhiteHat.Models;

namespace WhiteHat.Components
{
    public partial class ListView
    {
        private HnItemAlgolia _selectedItem;
        private int _selectedIndex = -1;
        private bool _isIframeLoading, _isFrameOpen, _isComments;
        private PaneSize _paneSize;
        private PersonalSettings _settings;
        private string _theme;

        [Parameter]
        public List<HnItemAlgolia> List { get; set; }
        
        [Parameter]
        public HnItemAlgolia SelectedItem 
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (value == _selectedItem)
                {
                    return;
                }
                _selectedItem = value;
                if (SelectedItemChanged.HasDelegate)
                {
                    SelectedItemChanged.InvokeAsync(value);
                }
            }
        }

        [Parameter]
        public EventCallback<HnItemAlgolia> SelectedItemChanged { get; set; }

        [Parameter]
        public bool IsLoading { get; set; }

        [Parameter]
        public bool NoMoreToLoad { get; set; }

        [Parameter]
        public EventCallback LoadMore { get; set; }

        [Parameter]
        public WhiteHatPages Origin { get; set; }

        protected async override Task OnInitializedAsync()
        {
            _settings = await Storage.GetSettings();
            _paneSize = _settings.PaneSize;
            var theme = await Storage.GetTheme();
            _theme = theme == ThemeMode.light ? "light" : "dark";
            await JSRuntime.InvokeVoidAsync("setupKeyboardShortcuts", DotNetObjectReference.Create(this));
        }
        private async Task OpenItem(HnItemAlgolia item, int i)
        {
            if (SelectedItem == item)
            {
                SelectedItem = null;
                _isFrameOpen = false;
            }
            else
            {
                _isIframeLoading = true;
                StateHasChanged();
                await IframeAutoProxy(item);

                SelectedItem = item;
                _selectedIndex = i;
            }
        }

        private async Task IframeAutoProxy(HnItemAlgolia item)
        { 
            if (item.EmbedType == EmbedType.Url)
            {
                var CORS = await IframeCheckerService.CanDisplayInIframe(item.Url.ToString());
                if (!CORS)
                {
                    item.EmbedType = EmbedType.Proxy;
                    item.EmbedUrl = EmbedHelper.UrlEmbedFromEmbedType(EmbedType.Proxy, item.Url.ToString());
                }
            }
        }

        private async Task Reload()
        {
            await JSRuntime.InvokeVoidAsync("reloadIframe");
        }

        private void CloseLinkPane()
        {
            SelectedItem = null;
        }

        private async Task OpenInNewTab()
        {
            await JSRuntime.InvokeVoidAsync("open", SelectedItem.Url, "_blank");
        }

        private async Task TogglePrevSize()
        {
            await SetPrevSize(_paneSize.Next());
        }
        private async Task SetPrevSize(PaneSize size)
        {
            _paneSize = size;
            _settings.PaneSize = _paneSize;
            await Storage.UpdateSettings(_settings);
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

        /// <summary>
        /// Gets the icon to use in the pane size button.
        /// The icon shows the size of the pane that will appear - not current
        /// </summary>
        /// <returns></returns>
        private string GetPaneButtonSizeIcon()
        {
            switch (_paneSize.Next())
            {
                case PaneSize.Split:
                    return "vertical_split";
                case PaneSize.Slim:
                    return "unfold_less";
                case PaneSize.Wide:
                    return "unfold_more";
            }
            return "";
        }

        /// <summary>
        /// Gets the Text to use in the pane size button.
        /// The text describes the size of the pane that will appear - not current
        /// </summary>
        /// <returns></returns>
        private string GetPaneButtonSizeText()
        {
            switch (_paneSize.Next())
            {
                case PaneSize.Split:
                    return "Split";
                case PaneSize.Slim:
                    return "Slim";
                case PaneSize.Wide:
                    return "Wide";
            }
            return "";
        }



        [JSInvokable]
        public async Task HandleUp(bool select = false)
        {
            if (List is null)
                return;

            if (_selectedIndex > 0)
            {
                _selectedIndex--;
            }
            var item = List[_selectedIndex];
            await SelectItem(item);

            if (select && item.Url is not null)
            {
                SelectedItem = item;
                StateHasChanged();
            }

            await SelectItem(List[_selectedIndex]);
        }

        [JSInvokable]
        public async Task HandleDown(bool select = false)
        {
            if (List is null)
                return;

            if (_selectedIndex < List.Count - 1)
            {
                _selectedIndex++;
            }
            var item = List[_selectedIndex];
            await SelectItem(item);

            if (select && item.Url is not null)
            {
                SelectedItem = item;
                StateHasChanged();
            }

            if (_selectedIndex >= List.Count - 1 && !IsLoading)
            {
                await LoadStories();
            }
        }

        public async Task LoadStories()
        {
            if (LoadMore.HasDelegate)
            {
                await LoadMore.InvokeAsync();
            }
        }

        [JSInvokable]
        public void HandleCommentToggle()
        {
            if (_selectedIndex > -1)
            {
                var item = List[_selectedIndex];
                item.ShowComments = !item.ShowComments;
                StateHasChanged();
            }
        }
        [JSInvokable]
        public void HandlePaneCommentModeToggle()
        {
            ToggleContent();
            if (SelectedItem is not null)
            {
                StateHasChanged();
            }
        }

        [JSInvokable]
        public void HandleExpand()
        {
            if (_selectedIndex > -1)
            {
                var item = List[_selectedIndex];
                item.ShowExpand = !item.ShowExpand;
                StateHasChanged();
            }
        }

        [JSInvokable]
        public async Task HandlePreviewToggle()
        {
            if (_selectedIndex > -1)
            {
                var item = List[_selectedIndex];

                if (item is null || item.EmbedType == EmbedType.NoUrl)
                    return;

                if (SelectedItem == item)
                {
                    SelectedItem = null;
                    _isFrameOpen = false;
                }
                else
                {
                    await IframeAutoProxy(item);
                    SelectedItem = item;
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
                var item = List[_selectedIndex];

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

        private async Task DismissWelcome()
        {
            var settings = await Storage.GetSettings();
            settings.DismissWelcome = true;
            _settings = settings;
            await Storage.UpdateSettings(settings);
        }


        public void Dispose()
        {
            // Remove the event listener when the component is disposed
            JSRuntime.InvokeVoidAsync("removeKeyboardShortcuts");
        }
    }
}