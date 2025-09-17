using Prayerify.ViewModels;

namespace Prayerify.Pages
{
	public partial class EditPrayerPage : ContentPage, IQueryAttributable
	{
		private readonly EditPrayerViewModel _viewModel;

		public string? PrayerId { get; set; }

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

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
			if (query.ContainsKey("Id"))
			{
                PrayerId = query["Id"].ToString();
            } 
        }
    }
}


