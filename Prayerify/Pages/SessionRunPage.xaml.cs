using Prayerify.ViewModels;

namespace Prayerify.Pages
{
	[QueryProperty(nameof(Ids), nameof(Ids))]
	public partial class SessionRunPage : ContentPage
	{
		public string Ids { get; set; } = string.Empty;

		public SessionRunPage(SessionRunViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}
	}
}


