using CommunityToolkit.Mvvm.ComponentModel;

namespace Prayerify.ViewModels
{
	public partial class BaseViewModel : ObservableObject
	{
		[ObservableProperty]
		private bool _isBusy;

		[ObservableProperty]
		private string _title = string.Empty;
	}
}


