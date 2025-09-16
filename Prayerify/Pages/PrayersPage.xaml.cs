using Prayerify.ViewModels;

namespace Prayerify.Pages
{
	public partial class PrayersPage : ContentPage
	{
		private readonly PrayersViewModel _viewModel;

		public PrayersPage(PrayersViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = _viewModel = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await _viewModel.LoadAsync();
		}
	}
}


