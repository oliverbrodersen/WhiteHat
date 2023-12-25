using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;
using WhiteHat.Models;

namespace WhiteHat.Components
{
    public partial class HnListItem
    {
        [Parameter]
        public HnItemAlgolia Item { get; set; }

        [Parameter]
        public EventCallback<HnItemAlgolia> ItemChanged { get; set; }

        [Parameter]
        public int Index { get; set; }

        [Parameter]
        public int SelectedIndex { get; set; }

        [Parameter]
        public bool Loader { get; set; }

        [Parameter]
        public EventCallback<HnItemAlgolia> OpenLink { get; set; }

        [Parameter]
        public HnItemAlgolia Selected { get; set; }

        public int MyProperty { get; set; }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !Loader)
            {
                Item.CountKids();
                StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task ToggleExpand()
        {
            if (!string.IsNullOrWhiteSpace(Item.Text))
            {
                Item.ShowExpand = !Item.ShowExpand;
                if(ItemChanged.HasDelegate)
                {
                    await ItemChanged.InvokeAsync(Item);
                }
            }
        }

        private async Task ToggleComments()
        {
            Item.ShowComments = !Item.ShowComments;

            if (ItemChanged.HasDelegate)
            {
                await ItemChanged.InvokeAsync(Item);
            }
        }

        private string StripHTML(string HTMLText)
        {
            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            return reg.Replace(HTMLText, "");
        }

        private async Task ItemClicked()
        {
            if (Item is null || Item.Url is null || string.IsNullOrEmpty(Item.Url.ToString()) || !OpenLink.HasDelegate)
            {
                return;
            }
            await OpenLink.InvokeAsync(Item);
        }
    }
}