using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace Prayerify.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        [ObservableProperty]
        private bool _darkModeEnabled = Preferences.Get("theme", "light") == "dark";

        public SettingsViewModel()
        {
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
    }
}
