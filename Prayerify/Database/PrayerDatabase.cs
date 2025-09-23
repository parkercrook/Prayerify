using SQLite;
using Prayerify.Models;
using SQLitePCL;

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
		Task<int> TogglePrayerAnsweredAsync(int id, bool isAnswered);
        Task<int> UpsertCategoryAsync(Category category);
		Task<int> DeleteCategoryAsync(int id);
		Task<List<Category>> GetCategoriesAsync();
		Task<Category> GetCategoryAsync(int id);
		Task<int> UpdatePrayerCountAsync();
		Task ClearAllDataAsync();

    }

	public class PrayerDatabase : IPrayerDatabase
	{
		private readonly SQLiteAsyncConnection _connection;
		private const int CurrentDatabaseVersion = 2;

		public PrayerDatabase(string databasePath)
		{
			try
			{
				// Ensure the directory exists
				var directory = Path.GetDirectoryName(databasePath);
				if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				// Use simple connection string with explicit provider
				_connection = new SQLiteAsyncConnection(databasePath);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to create SQLite connection: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"Database path: {databasePath}");
				System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException?.Message}");
				throw;
			}
		}

		public async Task InitializeAsync()
		{
			try
			{
				// Create tables
				await _connection.CreateTableAsync<Prayer>();
				await _connection.CreateTableAsync<Category>();
				
				// Only run migrations for existing databases
				await RunMigrationsAsync();
			}
			catch (Exception ex)
			{
				// Log the error and rethrow to prevent silent failures
				System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
				
				// If it's a type initializer error, try to reinitialize SQLite
				if (ex.Message.Contains("type initializer"))
				{
					System.Diagnostics.Debug.WriteLine("Attempting to reinitialize SQLite...");
					try
					{
						SQLitePCL.Batteries_V2.Init();
						
						// Try again after reinitialization
						await _connection.CreateTableAsync<Prayer>();
						await _connection.CreateTableAsync<Category>();
						await RunMigrationsAsync();
						return;
					}
					catch (Exception ex2)
					{
						System.Diagnostics.Debug.WriteLine($"SQLite reinitialization failed: {ex2.Message}");
					}
				}
				
				throw;
			}
		}

		private async Task RunMigrationsAsync()
		{
			try
			{
				// Get current database version
				var currentVersion = await GetDatabaseVersionAsync();
				
				// Run migrations if needed
				if (currentVersion < 1)
				{
					await MigrateToVersion1Async();
					await SetDatabaseVersionAsync(1);
				}
				
				if (currentVersion < 2)
				{
					await MigrateToVersion2Async();
					await SetDatabaseVersionAsync(2);
				}
			}
			catch (Exception ex)
			{
				// Log the error and rethrow to prevent silent failures
				System.Diagnostics.Debug.WriteLine($"Migration failed: {ex.Message}");
				throw;
			}
		}

		private async Task<int> GetDatabaseVersionAsync()
		{
			try
			{
				var result = await _connection.ExecuteScalarAsync<int>("PRAGMA user_version");
				return result;
			}
			catch
			{
				return 0; // Database doesn't exist or is very old
			}
		}

		private async Task SetDatabaseVersionAsync(int version)
		{
			await _connection.ExecuteAsync($"PRAGMA user_version = {version}");
		}

		private async Task MigrateToVersion1Async()
		{
			// Version 1 migration - tables are already created by CreateTableAsync
			// This is just a placeholder for future migrations
			await Task.CompletedTask;
		}

		private async Task MigrateToVersion2Async()
		{
			try
			{
				// Migration to version 2: Rename columns from Subject->PrayerTitle and Body->PrayerDescription
				// Since SQLite doesn't support ALTER COLUMN, we need to recreate the table
				
				// Check if we need to migrate by trying to query the old columns
				bool hasOldColumns = false;
				try
				{
					// Try to select from old columns - if this fails, columns don't exist
					await _connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Prayer WHERE Subject IS NOT NULL OR Body IS NOT NULL");
					hasOldColumns = true;
				}
				catch
				{
					// Old columns don't exist, so no migration needed
					hasOldColumns = false;
				}
				
				if (hasOldColumns)
				{
					await _connection.RunInTransactionAsync(conn =>
					{
						// Create temporary table with new schema
						conn.Execute(@"
							CREATE TABLE Prayer_temp (
								Id INTEGER PRIMARY KEY AUTOINCREMENT,
								CategoryId INTEGER,
								PrayerTitle TEXT NOT NULL,
								PrayerDescription TEXT NOT NULL,
								IsAnswered INTEGER NOT NULL DEFAULT 0,
								IsDeleted INTEGER NOT NULL DEFAULT 0,
								CreatedUtc TEXT NOT NULL
							)");
						
						// Copy data from old table to new table, mapping old column names to new ones
						conn.Execute(@"
							INSERT INTO Prayer_temp (Id, CategoryId, PrayerTitle, PrayerDescription, IsAnswered, IsDeleted, CreatedUtc)
							SELECT Id, CategoryId, 
							       COALESCE(Subject, '') as PrayerTitle,
							       COALESCE(Body, '') as PrayerDescription,
							       IsAnswered, IsDeleted, CreatedUtc
							FROM Prayer");
						
						// Drop old table
						conn.Execute("DROP TABLE Prayer");
						
						// Rename new table
						conn.Execute("ALTER TABLE Prayer_temp RENAME TO Prayer");
						
						// Recreate indexes
						conn.Execute("CREATE INDEX IF NOT EXISTS IX_Prayer_CategoryId ON Prayer(CategoryId)");
					});
				}
			}
			catch (Exception ex)
			{
				// Log the error and rethrow to prevent silent failures
				System.Diagnostics.Debug.WriteLine($"Migration to version 2 failed: {ex.Message}");
				throw;
			}
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

		public Task<int> TogglePrayerAnsweredAsync(int id, bool isAnswered)
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

		public async Task ClearAllDataAsync()
		{
			try
			{
				// Clear all prayers
				await _connection.ExecuteAsync("DELETE FROM Prayer");
				
				// Clear all categories
				await _connection.ExecuteAsync("DELETE FROM Category");
				
				// Reset the database version to 0 so it will be reinitialized
				await _connection.ExecuteAsync("PRAGMA user_version = 0");
				
				// Clear any app preferences that might contain user data
				Preferences.Clear();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to clear all data: {ex.Message}");
				throw;
			}
		}
	}
}


