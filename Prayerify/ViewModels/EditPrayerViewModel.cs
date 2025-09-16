using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prayerify.Data;
using Prayerify.Models;
using System.Collections.ObjectModel;

namespace Prayerify.ViewModels
{
	public partial class EditPrayerViewModel : BaseViewModel
	{
		private readonly IPrayerDatabase _database;

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

		public EditPrayerViewModel(IPrayerDatabase database)
		{
			_database = database;
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
			var model = new Prayer
			{
				Id = Id,
				Subject = Subject,
				Body = Body,
				CategoryId = SelectedCategory?.Id,
			};
			await _database.UpsertPrayerAsync(model);
			await Shell.Current.GoToAsync("..");
		}
	}
}


