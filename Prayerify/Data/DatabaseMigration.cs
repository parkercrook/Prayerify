using SQLite;

namespace Prayerify.Data
{
    public class DatabaseMigration
    {
        private readonly SQLiteAsyncConnection _connection;
        private const string VersionTableName = "SchemaVersion";

        public DatabaseMigration(SQLiteAsyncConnection connection)
        {
            _connection = connection;
        }

        public async Task MigrateAsync()
        {
            // Create version tracking table if it doesn't exist
            await CreateVersionTableAsync();

            // Get current version
            var currentVersion = await GetCurrentVersionAsync();

            // Apply migrations based on current version
            await ApplyMigrationsAsync(currentVersion);
        }

        private async Task CreateVersionTableAsync()
        {
            await _connection.ExecuteAsync($@"
                CREATE TABLE IF NOT EXISTS {VersionTableName} (
                    Version INTEGER PRIMARY KEY
                )");
        }

        private async Task<int> GetCurrentVersionAsync()
        {
            try
            {
                var result = await _connection.QueryAsync<int>($"SELECT Version FROM {VersionTableName} ORDER BY Version DESC LIMIT 1");
                return result.FirstOrDefault();
            }
            catch
            {
                return 0; // No version table or no records means version 0
            }
        }

        private async Task SetVersionAsync(int version)
        {
            await _connection.ExecuteAsync($"INSERT OR REPLACE INTO {VersionTableName} (Version) VALUES (?)", version);
        }

        private async Task ApplyMigrationsAsync(int currentVersion)
        {
            // Migration 1: Rename PrayerTitle to Subject and PrayerDescription to Body
            if (currentVersion < 1)
            {
                await MigrateToVersion1Async();
                await SetVersionAsync(1);
            }

            // Add more migrations here as needed in the future
            // if (currentVersion < 2)
            // {
            //     await MigrateToVersion2Async();
            //     await SetVersionAsync(2);
            // }
        }

        private async Task MigrateToVersion1Async()
        {
            try
            {
                // Check if the old columns exist
                var tableInfo = await _connection.QueryAsync<dynamic>("PRAGMA table_info(Prayer)");
                var hasOldColumns = tableInfo.Any(c => c.name == "Subject") && 
                                   tableInfo.Any(c => c.name == "Body");
                var hasNewColumns = tableInfo.Any(c => c.name == "PrayerTitle") && 
                                   tableInfo.Any(c => c.name == "PrayerDescription");

                if (hasOldColumns && !hasNewColumns)
                {
                    // Need to migrate from old schema (Subject/Body) to new schema (PrayerTitle/PrayerDescription)
                    await _connection.ExecuteAsync(@"
                        ALTER TABLE Prayer ADD COLUMN PrayerTitle TEXT;
                        ALTER TABLE Prayer ADD COLUMN PrayerDescription TEXT;
                    ");

                    // Copy data from old columns to new columns
                    await _connection.ExecuteAsync(@"
                        UPDATE Prayer SET 
                            PrayerTitle = Subject,
                            PrayerDescription = Body
                        WHERE PrayerTitle IS NULL OR PrayerDescription IS NULL;
                    ");

                    // Note: SQLite doesn't support dropping columns directly
                    // The old columns will remain but won't be used by the new model
                    Console.WriteLine("Migration to version 1 completed: Renamed Subject->PrayerTitle and Body->PrayerDescription");
                }
                else if (!hasOldColumns && hasNewColumns)
                {
                    // Already using new schema, nothing to migrate
                    Console.WriteLine("Database already at version 1 or higher");
                }
                else
                {
                    // Neither old nor new columns exist - this is a fresh database
                    Console.WriteLine("Fresh database detected - no migration needed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration error: {ex.Message}");
                throw;
            }
        }

        // Helper method to check if a column exists in a table
        private async Task<bool> ColumnExistsAsync(string tableName, string columnName)
        {
            var tableInfo = await _connection.QueryAsync<dynamic>($"PRAGMA table_info({tableName})");
            return tableInfo.Any(c => c.name == columnName);
        }
    }
}
