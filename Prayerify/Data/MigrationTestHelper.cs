using SQLite;

namespace Prayerify.Data
{
    public class MigrationTestHelper
    {
        private readonly SQLiteAsyncConnection _connection;

        public MigrationTestHelper(SQLiteAsyncConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Creates test data with the old schema for migration testing
        /// </summary>
        public async Task CreateOldSchemaTestDataAsync()
        {
            // Create Prayer table with old schema (Subject/Body) if it doesn't exist
            await _connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS Prayer_Old (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CategoryId INTEGER,
                    Subject TEXT NOT NULL,
                    Body TEXT NOT NULL,
                    IsAnswered INTEGER NOT NULL DEFAULT 0,
                    IsDeleted INTEGER NOT NULL DEFAULT 0,
                    CreatedUtc TEXT NOT NULL DEFAULT 'January 01, 2024'
                )");

            // Insert some test data with old schema
            await _connection.ExecuteAsync(@"
                INSERT OR REPLACE INTO Prayer_Old (Id, CategoryId, Subject, Body, IsAnswered, IsDeleted, CreatedUtc)
                VALUES 
                    (1, 1, 'Test Prayer 1', 'This is a test prayer description for migration testing.', 0, 0, 'January 15, 2024'),
                    (2, NULL, 'Test Prayer 2', 'Another test prayer for migration verification.', 1, 0, 'January 16, 2024'),
                    (3, 2, 'Test Prayer 3', 'Third test prayer to ensure migration works correctly.', 0, 0, 'January 17, 2024')
            ");
        }

        /// <summary>
        /// Simulates the old database schema by renaming the current table
        /// </summary>
        public async Task SimulateOldDatabaseAsync()
        {
            try
            {
                // Check if we have a current Prayer table
                var tableInfo = await _connection.QueryAsync<dynamic>("PRAGMA table_info(Prayer)");
                if (tableInfo.Any())
                {
                    // Backup current table
                    await _connection.ExecuteAsync("ALTER TABLE Prayer RENAME TO Prayer_Backup");
                    
                    // Create old schema table (Subject/Body instead of PrayerTitle/PrayerDescription)
                    await _connection.ExecuteAsync(@"
                        CREATE TABLE Prayer (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            CategoryId INTEGER,
                            Subject TEXT NOT NULL,
                            Body TEXT NOT NULL,
                            IsAnswered INTEGER NOT NULL DEFAULT 0,
                            IsDeleted INTEGER NOT NULL DEFAULT 0,
                            CreatedUtc TEXT NOT NULL DEFAULT 'January 01, 2024'
                        )");

                    // Copy data from backup, mapping new columns to old
                    await _connection.ExecuteAsync(@"
                        INSERT INTO Prayer (Id, CategoryId, Subject, Body, IsAnswered, IsDeleted, CreatedUtc)
                        SELECT Id, CategoryId, PrayerTitle, PrayerDescription, IsAnswered, IsDeleted, CreatedUtc
                        FROM Prayer_Backup
                    ");

                    // Drop the backup table
                    await _connection.ExecuteAsync("DROP TABLE Prayer_Backup");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error simulating old database: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Verifies that the migration worked correctly
        /// </summary>
        public async Task<bool> VerifyMigrationAsync()
        {
            try
            {
                // Check if new columns exist (PrayerTitle, PrayerDescription)
                var tableInfo = await _connection.QueryAsync<dynamic>("PRAGMA table_info(Prayer)");
                var hasPrayerTitle = tableInfo.Any(c => c.name == "PrayerTitle");
                var hasPrayerDescription = tableInfo.Any(c => c.name == "PrayerDescription");
                
                if (!hasPrayerTitle || !hasPrayerDescription)
                {
                    Console.WriteLine("Migration verification failed: New columns (PrayerTitle, PrayerDescription) not found");
                    return false;
                }

                // Check if data exists in new columns
                var prayers = await _connection.QueryAsync<dynamic>("SELECT Id, PrayerTitle, PrayerDescription FROM Prayer LIMIT 5");
                if (!prayers.Any())
                {
                    Console.WriteLine("Migration verification: No data found in Prayer table");
                    return true; // Empty database is valid
                }

                // Verify data integrity
                foreach (var prayer in prayers)
                {
                    if (string.IsNullOrEmpty(prayer.PrayerTitle?.ToString()) || string.IsNullOrEmpty(prayer.PrayerDescription?.ToString()))
                    {
                        Console.WriteLine($"Migration verification failed: Empty PrayerTitle or PrayerDescription for prayer ID {prayer.Id}");
                        return false;
                    }
                }

                Console.WriteLine("Migration verification passed: All data migrated successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration verification error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets database schema information for debugging
        /// </summary>
        public async Task<string> GetSchemaInfoAsync()
        {
            try
            {
                var tableInfo = await _connection.QueryAsync<dynamic>("PRAGMA table_info(Prayer)");
                var columns = string.Join(", ", tableInfo.Select(c => $"{c.name} ({c.type})"));
                
                var versionInfo = await _connection.QueryAsync<dynamic>("SELECT Version FROM SchemaVersion ORDER BY Version DESC LIMIT 1");
                var currentVersion = versionInfo.FirstOrDefault()?.Version ?? 0;

                return $"Prayer table columns: {columns}\nCurrent schema version: {currentVersion}";
            }
            catch (Exception ex)
            {
                return $"Error getting schema info: {ex.Message}";
            }
        }

        /// <summary>
        /// Cleans up test data
        /// </summary>
        public async Task CleanupTestDataAsync()
        {
            try
            {
                await _connection.ExecuteAsync("DROP TABLE IF EXISTS Prayer_Old");
                await _connection.ExecuteAsync("DROP TABLE IF EXISTS Prayer_Backup");
                Console.WriteLine("Test data cleaned up successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up test data: {ex.Message}");
            }
        }
    }
}
