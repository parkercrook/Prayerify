using Prayerify.ViewModels;

namespace Prayerify.Pages;

public partial class SettingsPage : ContentPage
{
	private readonly SettingsViewModel _viewModel;
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}
}