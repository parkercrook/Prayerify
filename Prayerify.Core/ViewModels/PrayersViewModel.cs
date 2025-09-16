using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prayerify.Core.Data;
using Prayerify.Core.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Prayerify.Core.ViewModels
{
	public partial class PrayersViewModel : BaseViewModel
	{
		private readonly IPrayerDatabase _database;
		private List<Prayer> _allPrayers = new();

		public ObservableCollection<Prayer> Prayers { get; } = new();
		public ObservableCollection<Category> Categories { get; } = new();

		[ObservableProperty]
		private int _sessionCount = 1;

		[ObservableProperty]
		private string _searchText = string.Empty;

		[ObservableProperty]
		private Category? _selectedCategory;

		[ObservableProperty]
		private bool _isSearchVisible = false;

		public PrayersViewModel(IPrayerDatabase database)
		{
			_database = database;
			Title = "Prayers";
			
			// Subscribe to property changes for real-time filtering
			PropertyChanged += OnPropertyChanged;
		}

		private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(SearchText) || e.PropertyName == nameof(SelectedCategory))
			{
				FilterPrayers();
			}
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
				
				// Load all prayers and categories
				_allPrayers = await _database.GetPrayersAsync(includeAnswered: false, includeDeleted: false);
				var categories = await _database.GetCategoriesAsync();
				
				// Add categories to collection
				foreach (var c in categories) Categories.Add(c);
				
				// Apply current filters
				FilterPrayers();
			}
			finally
			{
				IsBusy = false;
			}
		}

		private void FilterPrayers()
		{
			Prayers.Clear();
			
			var filteredPrayers = _allPrayers.AsEnumerable();
			
			// Apply category filter
			if (SelectedCategory != null)
			{
				filteredPrayers = filteredPrayers.Where(p => p.CategoryId == SelectedCategory.Id);
			}
			
			// Apply search filter
			if (!string.IsNullOrWhiteSpace(SearchText))
			{
				var searchLower = SearchText.ToLowerInvariant();
				filteredPrayers = filteredPrayers.Where(p => 
					p.Subject.ToLowerInvariant().Contains(searchLower) ||
					p.Body.ToLowerInvariant().Contains(searchLower) ||
					GetCategoryName(p.CategoryId).ToLowerInvariant().Contains(searchLower));
			}
			
			// Add filtered prayers to collection with category names set
			foreach (var prayer in filteredPrayers.OrderByDescending(p => p.CreatedUtc))
			{
				prayer.CategoryName = GetCategoryName(prayer.CategoryId);
				Prayers.Add(prayer);
			}
		}

		private string GetCategoryName(int? categoryId)
		{
			if (categoryId == null) return "No Category";
			var category = Categories.FirstOrDefault(c => c.Id == categoryId);
			return category?.Name ?? "Unknown Category";
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
				_allPrayers.Remove(prayer);
			}
		}

		[RelayCommand]
		public void ToggleSearchVisibility()
		{
			IsSearchVisible = !IsSearchVisible;
			if (!IsSearchVisible)
			{
				ClearFilters();
			}
		}

		[RelayCommand]
		public void ClearFilters()
		{
			SearchText = string.Empty;
			SelectedCategory = null;
		}

		[RelayCommand]
		public void ClearSearch()
		{
			SearchText = string.Empty;
		}
	}
}

