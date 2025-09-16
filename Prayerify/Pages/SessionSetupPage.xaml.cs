using Prayerify.ViewModels;

namespace Prayerify.Pages
{
	public partial class SessionSetupPage : ContentPage
	{
		public SessionSetupPage(SessionSetupViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}
	}
}


