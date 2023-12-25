using Microsoft.AspNetCore.Components;
using System.Runtime;
using System.Xml.Serialization;
using WhiteHat.Models;
using WhiteHat.Services;

namespace WhiteHat.Shared
{
    public partial class MainLayout
    {
        private string _search;
        PersonalSettings _settings;

        protected override async Task OnInitializedAsync()
        {
            _settings = await Storage.GetSettings();
        }

        private void Search()
        {
            if (!string.IsNullOrWhiteSpace(_search))
            {
                NavigationManager.NavigateTo("/search/" + _search);
                _search = string.Empty;
            }
        }

        private async Task ToggleShortcuts()
        {
            var settings = await Storage.GetSettings();
            settings.ShowShortcuts = !settings.ShowShortcuts;
            _settings = settings;
            await Storage.UpdateSettings(settings);
        }
    }
}