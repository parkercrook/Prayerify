using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prayerify.Core.Data;
using Prayerify.Core.Models;
using System.Collections.ObjectModel;

namespace Prayerify.Core.ViewModels
{
	public partial class PrayersViewModel : BaseViewModel
	{
		private readonly IPrayerDatabase _database;

		public ObservableCollection<Prayer> Prayers { get; } = new();
		public ObservableCollection<Category> Categories { get; } = new();

		[ObservableProperty]
		private int _sessionCount = 1;

		public PrayersViewModel(IPrayerDatabase database)
		{
			_database = database;
			Title = "Prayers";
		}

		[RelayCommand]
		public async Task LoadAsync()
		{
			if (IsBusy) return;
			try
			{
				IsBusy = true;
				Prayers.Clear();
				Categories.Clear();
				var prayers = await _database.GetPrayersAsync(includeAnswered: false, includeDeleted: false);
				var categories = await _database.GetCategoriesAsync();
				foreach (var c in categories) Categories.Add(c);
				foreach (var p in prayers) Prayers.Add(p);
			}
			finally
			{
				IsBusy = false;
			}
		}

		[RelayCommand]
		public async Task DeletePrayerAsync(Prayer prayer)
		{
			if (prayer == null) return;
			await _database.DeletePrayerAsync(prayer.Id);
			Prayers.Remove(prayer);
		}

		[RelayCommand]
		public async Task ToggleAnsweredAsync(Prayer prayer)
		{
			if (prayer == null) return;
			var newValue = !prayer.IsAnswered;
			await _database.MarkPrayerAnsweredAsync(prayer.Id, newValue);
			prayer.IsAnswered = newValue;
			if (newValue)
			{
				Prayers.Remove(prayer);
			}
		}
	}
}
