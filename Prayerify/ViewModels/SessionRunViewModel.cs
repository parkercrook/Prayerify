using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prayerify.Data;
using Prayerify.Models;

namespace Prayerify.ViewModels
{
	[QueryProperty(nameof(Ids), nameof(Ids))]
	public partial class SessionRunViewModel : BaseViewModel
	{
		private readonly IPrayerDatabase _database;

		[ObservableProperty]
		private string _ids = string.Empty;

		[ObservableProperty]
		private Prayer? _currentPrayer;

		[ObservableProperty]
		private Category? _currentCategory;

		private Queue<int> _queue = new();

		public SessionRunViewModel(IPrayerDatabase database)
		{
			_database = database;
			Title = "Session";
		}

		partial void OnIdsChanged(string value)
		{
			var arr = (value ?? string.Empty)
				.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(s => int.TryParse(s, out var n) ? n : 0)
				.Where(n => n > 0);
			_queue = new Queue<int>(arr);
			MainThread.BeginInvokeOnMainThread(async () => await LoadNextAsync());
		}

		[RelayCommand]
		public async Task LoadNextAsync()
		{
			if (_queue.Count == 0)
			{
				await Shell.Current.GoToAsync("..");
				return;
			}
			var id = _queue.Dequeue();
			CurrentPrayer = await _database.GetPrayerAsync(id);
			CurrentCategory = await _database.GetCategoryAsync(id);
		}

		[RelayCommand]
		public async Task MarkDoneAsync()
		{
			await LoadNextAsync();
		}

		[RelayCommand]
		public async Task MarkAnsweredAsync()
		{
			if (CurrentPrayer != null)
			{
				await _database.MarkPrayerAnsweredAsync(CurrentPrayer.Id, true);
			}
			await LoadNextAsync();
		}
	}
}


