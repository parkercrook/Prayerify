using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prayerify.Data;
using Prayerify.Models;
using Prayerify.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Prayerify.ViewModels
{
	public partial class AnsweredPrayersViewModel : BaseViewModel
	{
		private readonly IPrayerDatabase _database;
		private readonly IGenericService _genericService;

		public ObservableCollection<Prayer> Prayers { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        public AnsweredPrayersViewModel(IPrayerDatabase database, IGenericService genericService)
		{
			_database = database;
			_genericService = genericService;
			Title = "Answered";
		}

		[RelayCommand]
		public async Task LoadAnsweredPrayersAsync()
		{
			if (IsBusy) return;
			try
			{
				IsBusy = true;
				Prayers.Clear();
				Categories.Clear();

				var items = await _database.GetAnsweredPrayersAsync();
				var categories = await _database.GetCategoriesAsync();

				foreach (var c in categories) Categories.Add(c);
				foreach (var p in items)
				{
					if (p.CategoryId != null)
					{
						p.CategoryName = GetCategoryName(p.CategoryId);
					}

					Prayers.Add(p);
				}
			}
			catch(Exception ex)
			{
				string message = "Couldn't load answered prayers";
                Debug.WriteLine($"{message} {ex.Message}");
				Debug.WriteLine(ex.StackTrace);
				await _genericService.ShowAlertAsync("Error", message);
			}
			finally
			{
				IsBusy = false;
			}
		}
        private string GetCategoryName(int? categoryId)
        {
            if (categoryId == null) return "No Category";
            var category = Categories.FirstOrDefault(c => c.Id == categoryId);
            return category?.Name ?? "No Category";
        }

    }
}


