using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prayerify.Core.Data;
using Prayerify.Core.Models;
using Prayerify.Core.Services;
using System.Collections.ObjectModel;

namespace Prayerify.Core.ViewModels
{
	public partial class EditPrayerViewModel : BaseViewModel
	{
		private readonly IPrayerDatabase _database;
		private readonly IDialogService _dialogService;

		public ObservableCollection<Category> Categories { get; } = new();

		[ObservableProperty]
		private int _id;

		[ObservableProperty]
		private string _subject = string.Empty;

		[ObservableProperty]
		private string _body = string.Empty;

		[ObservableProperty]
		private int? _categoryId;

		[ObservableProperty]
		private Category? _selectedCategory;

		public EditPrayerViewModel(IPrayerDatabase database, IDialogService dialogService)
		{
			_database = database;
			_dialogService = dialogService;
			Title = "Edit Prayer";
		}

		public async Task LoadAsync(int? id)
		{
			// Load categories first
			Categories.Clear();
			var categories = await _database.GetCategoriesAsync();
			foreach (var c in categories) Categories.Add(c);

			if (id == null || id == 0)
			{
				Title = "Add Prayer";
				return;
			}
			var prayer = await _database.GetPrayerAsync(id.Value);
			if (prayer != null)
			{
				Id = prayer.Id;
				Subject = prayer.Subject;
				Body = prayer.Body;
				CategoryId = prayer.CategoryId;
				SelectedCategory = Categories.FirstOrDefault(c => c.Id == CategoryId);
			}
		}

		[RelayCommand]
		public async Task SaveAsync()
		{
			if(Subject == string.Empty)
			{
				await _dialogService.ShowAlertAsync("Empty Subject", "The subject of the prayer cannot be left empty.");
				return;
			}

            if (Body == string.Empty)
            {
                await _dialogService.ShowAlertAsync("Empty Body", "The body of the prayer cannot be left empty.");
				return;
            }

            var model = new Prayer
			{
				Id = Id,
				Subject = Subject,
				Body = Body,
				CategoryId = SelectedCategory?.Id,
			};
			await _database.UpsertPrayerAsync(model);
		}
	}
}
