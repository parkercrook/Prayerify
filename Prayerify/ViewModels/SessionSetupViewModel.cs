using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Prayerify.Data;
using Prayerify.Messages;
using Prayerify.Pages;
using System.ComponentModel;

namespace Prayerify.ViewModels
{
	public partial class SessionSetupViewModel : BaseViewModel, IRecipient<PrayerCountChangedMessage>, IDisposable
	{
		private readonly IPrayerDatabase _database;

		[ObservableProperty]
		private int _count = 0;

		[ObservableProperty]
		private int _totalPrayerCount;

		[ObservableProperty]
		private bool _canStartSession = false;

		public SessionSetupViewModel(IPrayerDatabase database)
		{
			_database = database;
			Title = "Prayer Session";
			PropertyChanged += OnPropertyChanged;
			
			// Register for prayer count change messages
			WeakReferenceMessenger.Default.Register(this);
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
		
		/// <summary>
		/// Receives notification when prayer count changes
		/// </summary>
		public void Receive(PrayerCountChangedMessage message)
		{
			// Update the total prayer count
			TotalPrayerCount = message.NewCount;
			
			// Ensure Count doesn't exceed the maximum
			if (Count > TotalPrayerCount)
			{
				Count = TotalPrayerCount;
			}
		}
		
		// Clean up when this object is disposed
		public void Dispose()
		{
			WeakReferenceMessenger.Default.Unregister<PrayerCountChangedMessage>(this);
		}
    }
}


