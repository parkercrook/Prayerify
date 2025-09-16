using Xunit;

namespace Prayerify.Tests
{
    /// <summary>
    /// Test runner configuration and utilities
    /// </summary>
    public static class TestRunner
    {
        /// <summary>
        /// Creates a temporary file path for test databases
        /// </summary>
        /// <param name="prefix">Prefix for the temporary file</param>
        /// <returns>A unique temporary file path</returns>
        public static string CreateTempDbPath(string prefix = "test")
        {
            return Path.Combine(Path.GetTempPath(), $"{prefix}_{Guid.NewGuid()}.db3");
        }

        /// <summary>
        /// Cleans up a temporary file if it exists
        /// </summary>
        /// <param name="filePath">Path to the file to clean up</param>
        public static void CleanupTempFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // Ignore cleanup errors in tests
                }
            }
        }
    }
}

