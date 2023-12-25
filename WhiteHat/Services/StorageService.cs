using Blazored.LocalStorage;
using WhiteHat.Misc;
using WhiteHat.Models;

namespace WhiteHat.Services
{
    public class StorageService
    {
        private ILocalStorageService _localStorage;
        public PersonalSettings Settings { get; set; }

        public StorageService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task UpdateSettings(PersonalSettings settings = null)
        {
            if (settings is not null)
            {
                Settings = settings;
            }
            await _localStorage.SetItemAsync(Constants.SettingsKeyId, Settings);
        }  

        public async Task<PersonalSettings> LoadSettings()
        {
            if (await _localStorage.ContainKeyAsync(Constants.SettingsKeyId))
            {
                Settings = await _localStorage.GetItemAsync<PersonalSettings>(Constants.SettingsKeyId);
                return Settings;
            }
            else
            {
                Settings = new PersonalSettings();
                Settings.ShowShortcuts = true;
                await UpdateSettings(Settings);
            }
            return Settings;
        }

        public async Task<PersonalSettings> GetSettings()
        {
            if (Settings is null)
                Settings = await LoadSettings();

            return Settings;
        } 
    }
}
