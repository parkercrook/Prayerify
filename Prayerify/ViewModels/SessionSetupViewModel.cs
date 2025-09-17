using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prayerify.Data;
using Prayerify.Pages;

namespace Prayerify.ViewModels
{
	public partial class SessionSetupViewModel : BaseViewModel
	{
		private readonly IPrayerDatabase _database;

		[ObservableProperty]
		private int _count = 0;

		public SessionSetupViewModel(IPrayerDatabase database)
		{
			_database = database;
			Title = "Prayer Session";
		}

		[RelayCommand]
		public async Task StartAsync()
		{
			var prayers = await _database.GetPrayersAsync(includeAnswered: false, includeDeleted: false);
			var rnd = new Random();
			var selection = prayers.OrderBy(_ => rnd.Next()).Take(Math.Max(1, Count)).Select(p => p.Id).ToArray();
			var idsParam = string.Join(",", selection);
			await Shell.Current.GoToAsync($"{nameof(SessionRunPage)}?Ids={idsParam}");
		}
	}
}


