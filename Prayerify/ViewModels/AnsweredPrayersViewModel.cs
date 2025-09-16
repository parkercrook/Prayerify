using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prayerify.Data;
using Prayerify.Models;
using System.Collections.ObjectModel;

namespace Prayerify.ViewModels
{
	public partial class AnsweredPrayersViewModel : BaseViewModel
	{
		private readonly IPrayerDatabase _database;

		public ObservableCollection<Prayer> Prayers { get; } = new();

		public AnsweredPrayersViewModel(IPrayerDatabase database)
		{
			_database = database;
			Title = "Answered";
		}

		[RelayCommand]
		public async Task LoadAsync()
		{
			if (IsBusy) return;
			try
			{
				IsBusy = true;
				Prayers.Clear();
				var items = await _database.GetAnsweredPrayersAsync();
				foreach (var p in items) Prayers.Add(p);
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}


