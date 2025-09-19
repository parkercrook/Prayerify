using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Prayerify.Data;
using Prayerify.Messages;
using Prayerify.Models;
using Prayerify.Services;
using System.Collections.ObjectModel;

namespace Prayerify.ViewModels
{
	public partial class EditPrayerViewModel : BaseViewModel
	{
		private readonly IPrayerDatabase _database;
		private readonly IGenericService _dialogService;

		public ObservableCollection<Category> Categories { get; } = new();

		[ObservableProperty]
		private int _id;

		[ObservableProperty]
		private string _prayerTitle = string.Empty;

		[ObservableProperty]
		private string _prayerDescription = string.Empty;

		[ObservableProperty]
		private int? _categoryId;

		[ObservableProperty]
		private Category? _selectedCategory;

		public EditPrayerViewModel(IPrayerDatabase database, IGenericService dialogService)
		{
			_database = database;
			_dialogService = dialogService;
			Title = "Edit Prayer";
		}

		public async Task LoadAsync(string? idParam)
		{
			int.TryParse(idParam, out int id);

			// Load categories first
			Categories.Clear();
			var categories = await _database.GetCategoriesAsync();
			foreach (var c in categories) Categories.Add(c);

			if (id == 0)
			{
				Title = "Add Prayer";
				return;
			}
			var prayer = await _database.GetPrayerAsync(id);
			if (prayer != null)
			{
				Id = prayer.Id;
				PrayerTitle = prayer.PrayerTitle;
				PrayerDescription = prayer.PrayerDescription;
				CategoryId = prayer.CategoryId;
				SelectedCategory = Categories.FirstOrDefault(c => c.Id == CategoryId);
			}
		}

		[RelayCommand]
		public async Task SaveAsync()
		{
			if(PrayerTitle == string.Empty)
			{
				await _dialogService.ShowAlertAsync("Empty Subject", "The subject of the prayer cannot be left empty.");
				return;
			}

            var model = new Prayer
			{
				Id = Id,
				PrayerTitle = PrayerTitle,
				PrayerDescription = PrayerDescription,
				CategoryId = SelectedCategory?.Id,
			};
			await _database.UpsertPrayerAsync(model);
			var newCount = await _database.UpdatePrayerCountAsync();
			
			// Notify that prayer count has changed (only if adding a new prayer)
			if (Id == 0)
			{
				WeakReferenceMessenger.Default.Send(new PrayerCountChangedMessage(newCount));
			}
			
			await Shell.Current.GoToAsync("..");
		}
	}
}


