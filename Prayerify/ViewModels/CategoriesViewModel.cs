using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prayerify.Data;
using Prayerify.Models;
using Prayerify.Services;
using System.Collections.ObjectModel;

namespace Prayerify.ViewModels
{
	public partial class CategoriesViewModel : BaseViewModel
	{
		private readonly IPrayerDatabase _database;
		private readonly IDialogService _dialogService;

		public ObservableCollection<Category> Categories { get; } = new();

		[ObservableProperty]
		private string _newCategoryName = string.Empty;

		public CategoriesViewModel(IPrayerDatabase database, IDialogService dialogService)
		{
			_database = database;
			_dialogService = dialogService;
			Title = "Categories";
		}

		[RelayCommand]
		public async Task LoadAsync()
		{
			if (IsBusy) return;
			try
			{
				IsBusy = true;
				Categories.Clear();
				var items = await _database.GetCategoriesAsync();
				foreach (var c in items) Categories.Add(c);
			}
			finally
			{
				IsBusy = false;
			}
		}

		[RelayCommand]
		public async Task AddCategoryAsync()
		{
			if(NewCategoryName == string.Empty)
			{
				await _dialogService.ShowAlertAsync("Empty Category Name", "The category name cannot be left empty.");
				return;
			}

			var name = (NewCategoryName ?? string.Empty).Trim();
			if (string.IsNullOrWhiteSpace(name)) return;
			var cat = new Category { Name = name };
			await _database.UpsertCategoryAsync(cat);
			Categories.Insert(0, cat);
			NewCategoryName = string.Empty;
		}

		[RelayCommand]
		public async Task DeleteCategoryAsync(Category category)
		{
			if (category == null) return;
			await _database.DeleteCategoryAsync(category.Id);
			Categories.Remove(category);
		}
	}
}


