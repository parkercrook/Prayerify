using SQLite;
using Prayerify.Models;

namespace Prayerify.Data
{
	public interface IPrayerDatabase
	{
		Task InitializeAsync();
		Task<int> UpsertPrayerAsync(Prayer prayer);
		Task<int> DeletePrayerAsync(int id);
		Task<Prayer?> GetPrayerAsync(int id);
		Task<List<Prayer>> GetPrayersAsync(bool includeAnswered = true, bool includeDeleted = false);
		Task<List<Prayer>> GetAnsweredPrayersAsync();
		Task<List<Prayer>> GetPrayersByCategoryAsync(int? categoryId, bool includeAnswered = true);
		Task<int> MarkPrayerAnsweredAsync(int id, bool isAnswered);

		Task<int> UpsertCategoryAsync(Category category);
		Task<int> DeleteCategoryAsync(int id);
		Task<List<Category>> GetCategoriesAsync();
		Task<Category> GetCategoryAsync(int id);
		Task<int> UpdatePrayerCountAsync();

    }

	public class PrayerDatabase : IPrayerDatabase
	{
		private readonly SQLiteAsyncConnection _connection;

		public PrayerDatabase(string databasePath)
		{
			_connection = new SQLiteAsyncConnection(databasePath);
		}

		public async Task InitializeAsync()
		{
			await _connection.CreateTableAsync<Prayer>();
			await _connection.CreateTableAsync<Category>();
		}

		public Task<int> UpsertPrayerAsync(Prayer prayer)
		{
			if (prayer.Id == 0)
			{
				return _connection.InsertAsync(prayer);
			}
			return _connection.UpdateAsync(prayer);
		}

		public Task<int> DeletePrayerAsync(int id)
		{
			return _connection.ExecuteAsync("UPDATE Prayer SET IsDeleted = 1 WHERE Id = ?", id);
		}

		public async Task<Prayer?> GetPrayerAsync(int id)
		{
			var result = await _connection.Table<Prayer>().Where(p => p.Id == id).FirstOrDefaultAsync();
			return result;
		}

		public Task<List<Prayer>> GetPrayersAsync(bool includeAnswered = true, bool includeDeleted = false)
		{
			var answeredClause = includeAnswered ? string.Empty : " AND IsAnswered = 0";
			var deletedClause = includeDeleted ? string.Empty : " AND IsDeleted = 0";
			var sql = $"SELECT * FROM Prayer WHERE 1=1{answeredClause}{deletedClause} ORDER BY CreatedUtc DESC";
			return _connection.QueryAsync<Prayer>(sql);
		}

		public Task<List<Prayer>> GetPrayersByCategoryAsync(int? categoryId, bool includeAnswered = true)
		{
			var answeredClause = includeAnswered ? string.Empty : " AND IsAnswered = 0";
			if (categoryId == null)
			{
				return _connection.QueryAsync<Prayer>($"SELECT * FROM Prayer WHERE CategoryId IS NULL AND IsDeleted = 0{answeredClause} ORDER BY CreatedUtc DESC");
			}
			return _connection.QueryAsync<Prayer>($"SELECT * FROM Prayer WHERE CategoryId = ? AND IsDeleted = 0{answeredClause} ORDER BY CreatedUtc DESC", categoryId);
		}

		public Task<List<Prayer>> GetAnsweredPrayersAsync()
		{
			return _connection.QueryAsync<Prayer>("SELECT * FROM Prayer WHERE IsAnswered = 1 AND IsDeleted = 0 ORDER BY CreatedUtc DESC");
		}

		public Task<int> MarkPrayerAnsweredAsync(int id, bool isAnswered)
		{
			return _connection.ExecuteAsync("UPDATE Prayer SET IsAnswered = ? WHERE Id = ?", isAnswered ? 1 : 0, id);
		}

		public Task<int> UpsertCategoryAsync(Category category)
		{
			if (category.Id == 0)
			{
				return _connection.InsertAsync(category);
			}
			return _connection.UpdateAsync(category);
		}

		public Task<int> DeleteCategoryAsync(int id)
		{
			return _connection.ExecuteAsync("DELETE FROM Category WHERE Id = ?", id);
		}

		public Task<List<Category>> GetCategoriesAsync()
		{
			return _connection.Table<Category>().OrderByDescending(c => c.CreatedUtc).ToListAsync();
		}

		public async Task<Category> GetCategoryAsync(int id)
		{
            var result = await _connection.Table<Category>().Where(c => c.Id == id).FirstOrDefaultAsync();
            return result;
        }

		public async Task<int> UpdatePrayerCountAsync()
		{
			var activePrayers = await GetPrayersAsync(includeAnswered: false, includeDeleted: false);
			return activePrayers.Count;
		}
	}
}


