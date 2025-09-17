using Prayerify.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Prayerify.Pages
{
	public partial class SessionSetupPage : ContentPage
	{
        private readonly SessionSetupViewModel _viewModel;
        public SessionSetupPage(SessionSetupViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
			_viewModel = viewModel;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await _viewModel.LoadAsync();
		}

	}
}


