using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prayerify.Data;
using Prayerify.Pages;
using System.ComponentModel;

namespace Prayerify.ViewModels
{
	public partial class SessionSetupViewModel : BaseViewModel
	{
		private readonly IPrayerDatabase _database;

		[ObservableProperty]
		private int _count = 0;

		[ObservableProperty]
		private int _totalPrayerCount;

		[ObservableProperty]
		private bool _canStartSession = false;

		[ObservableProperty]
		private bool _canIncreaseCount = false;

        public SessionSetupViewModel(IPrayerDatabase database)
		{
			_database = database;
			Title = "Prayer Session";
			PropertyChanged += OnPropertyChanged;
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

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
				var result = await _database.UpdatePrayerCountAsync();
				TotalPrayerCount = result;
				
				// Ensure Count doesn't exceed the maximum
				if (Count > TotalPrayerCount)
				{
					Count = TotalPrayerCount;
				}

				CanIncreaseCount = TotalPrayerCount > 0 ? true : false;
            }
            finally
            {
                IsBusy = false;
            }
        }

		private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Count))
			{
				if(Count > 0)
				{
					CanStartSession = true;
				}
				else
				{
					CanStartSession = false;
				}
			}
		}
    }
}


