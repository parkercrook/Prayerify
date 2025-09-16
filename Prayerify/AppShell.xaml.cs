using Prayerify.Pages;

namespace Prayerify
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
			Routing.RegisterRoute(nameof(PrayersPage), typeof(PrayersPage));
			Routing.RegisterRoute(nameof(EditPrayerPage), typeof(EditPrayerPage));
			Routing.RegisterRoute(nameof(CategoriesPage), typeof(CategoriesPage));
			Routing.RegisterRoute(nameof(SessionSetupPage), typeof(SessionSetupPage));
			Routing.RegisterRoute(nameof(SessionRunPage), typeof(SessionRunPage));
			Routing.RegisterRoute(nameof(AnsweredPrayersPage), typeof(AnsweredPrayersPage));
		}
	}
}
