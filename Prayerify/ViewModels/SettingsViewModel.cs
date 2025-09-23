using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using Prayerify.Data;

namespace Prayerify.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly IPrayerDatabase _database;

        [ObservableProperty]
        private bool _darkModeEnabled = Preferences.Get("theme", "light") == "dark";

        public SettingsViewModel(IPrayerDatabase database)
        {
            _database = database;
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DarkModeEnabled))
            {
                ChangeTheme();
            }
        }

        private void ChangeTheme()
        {
            if (Application.Current == null) return;
            Console.WriteLine("Theme");

            if (DarkModeEnabled)
            {
                Application.Current.UserAppTheme = AppTheme.Dark;
                Preferences.Set("theme", "dark");
            }
            else
            {
                Application.Current.UserAppTheme = AppTheme.Light;
                Preferences.Set("theme", "light");
            }
        }

        [RelayCommand]
        private async Task ClearAllDataAsync()
        {
            try
            {
                // Show confirmation dialog
                bool confirmed = false;
                if (Application.Current?.MainPage != null)
                {
                    confirmed = await Application.Current.MainPage.DisplayAlert(
                        "Clear All Data",
                        "Are you sure you want to clear all your prayers and categories? This action cannot be undone.",
                        "Yes, Clear All",
                        "Cancel");
                }

                if (!confirmed)
                    return;

                // Show loading indicator
                IsBusy = true;

                // Clear all data
                await _database.ClearAllDataAsync();

                // Show success message
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Data Cleared",
                        "All your prayers and categories have been cleared successfully.",
                        "OK");
                }

                // Reset theme preference since it was cleared
                DarkModeEnabled = false;
            }
            catch (Exception ex)
            {
                // Show error message
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Failed to clear data: {ex.Message}",
                        "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
