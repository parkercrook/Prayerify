using Prayerify.ViewModels;

namespace Prayerify.Pages
{
	public partial class AnsweredPrayersPage : ContentPage
	{
		private readonly AnsweredPrayersViewModel _viewModel;

		public AnsweredPrayersPage(AnsweredPrayersViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = _viewModel = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await _viewModel.LoadAnsweredPrayersAsync();
		}
	}
}


