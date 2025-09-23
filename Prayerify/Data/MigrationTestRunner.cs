using SQLite;

namespace Prayerify.Data
{
    public class MigrationTestRunner
    {
        public static async Task RunMigrationTestAsync(string databasePath)
        {
            Console.WriteLine("=== Prayerify Database Migration Test ===");
            
            var connection = new SQLiteAsyncConnection(databasePath);
            var testHelper = new MigrationTestHelper(connection);

            try
            {
                // Step 1: Create test data with old schema (Subject/Body)
                Console.WriteLine("Step 1: Creating test data with old schema (Subject/Body)...");
                await testHelper.CreateOldSchemaTestDataAsync();
                Console.WriteLine("✓ Test data created");

                // Step 2: Simulate old database state
                Console.WriteLine("Step 2: Simulating old database schema (Subject/Body)...");
                await testHelper.SimulateOldDatabaseAsync();
                Console.WriteLine("✓ Old database schema simulated");

                // Step 3: Show current schema info
                Console.WriteLine("Step 3: Current schema info:");
                var schemaInfo = await testHelper.GetSchemaInfoAsync();
                Console.WriteLine(schemaInfo);

                // Step 4: Run migration
                Console.WriteLine("Step 4: Running migration...");
                var migration = new DatabaseMigration(connection);
                await migration.MigrateAsync();
                Console.WriteLine("✓ Migration completed");

                // Step 5: Verify migration
                Console.WriteLine("Step 5: Verifying migration...");
                var migrationSuccess = await testHelper.VerifyMigrationAsync();
                if (migrationSuccess)
                {
                    Console.WriteLine("✓ Migration verification passed!");
                }
                else
                {
                    Console.WriteLine("✗ Migration verification failed!");
                }

                // Step 6: Show final schema info
                Console.WriteLine("Step 6: Final schema info:");
                var finalSchemaInfo = await testHelper.GetSchemaInfoAsync();
                Console.WriteLine(finalSchemaInfo);

                // Step 7: Cleanup
                Console.WriteLine("Step 7: Cleaning up test data...");
                await testHelper.CleanupTestDataAsync();
                Console.WriteLine("✓ Test data cleaned up");

                Console.WriteLine("\n=== Migration Test Complete ===");
                Console.WriteLine(migrationSuccess ? "SUCCESS: Migration test passed!" : "FAILED: Migration test failed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Migration test failed with error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
