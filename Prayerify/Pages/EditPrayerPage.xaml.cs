using Prayerify.ViewModels;

namespace Prayerify.Pages
{
	[QueryProperty(nameof(PrayerId), nameof(PrayerId))]
	public partial class EditPrayerPage : ContentPage
	{
		private readonly EditPrayerViewModel _viewModel;

		public int? PrayerId { get; set; }

		public EditPrayerPage(EditPrayerViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = _viewModel = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await _viewModel.LoadAsync(PrayerId);
		}
	}
}


