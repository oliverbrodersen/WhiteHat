using Microsoft.AspNetCore.Components;
using System.Runtime;
using System.Xml.Serialization;
using WhiteHat.Misc;
using WhiteHat.Models;
using WhiteHat.Services;
using Version = WhiteHat.Models.Version;

namespace WhiteHat.Shared
{
    public partial class MainLayout
    {
        private bool _updated;
        private string _search;
        PersonalSettings _settings;

        protected override async Task OnInitializedAsync()
        {
            _settings = await Storage.GetSettings();
            if (_updated)
            {
                _updated = false;
            }
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (firstRender)
            {
                string lastVersion = await localStorage.GetItemAsync<string>(Constants.VersionKeyId);
                if (!Version.TryParse(lastVersion) || !Constants.CurrentVersion.Equals((Version)lastVersion))
                {
                    await localStorage.SetItemAsync(Constants.VersionKeyId, Constants.CurrentVersion.ToString());
                    _updated = true;
                    StateHasChanged();
                }
            }
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