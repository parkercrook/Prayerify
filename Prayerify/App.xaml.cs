namespace Prayerify
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            UserAppTheme = Preferences.Get("theme", "light") == "light" ? AppTheme.Light : AppTheme.Dark;

            MainPage = new AppShell();
        }
    }
}
