using FluentAssertions;
using Prayerify.Core.Data;
using Prayerify.Core.Models;
using SQLite;
using Xunit;

namespace Prayerify.Tests.Data
{
    public class PrayerDatabaseTests : IDisposable
    {
        private readonly PrayerDatabase _database;
        private readonly string _testDbPath;

        public PrayerDatabaseTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_prayerify_{Guid.NewGuid()}.db3");
            _database = new PrayerDatabase(_testDbPath);
        }

        [Fact]
        public async Task InitializeAsync_ShouldCreateTables()
        {
            // Act
            await _database.InitializeAsync();

            // Assert - If no exception is thrown, tables were created successfully
            // We can verify by trying to insert data
            var prayer = new Prayer { Subject = "Test", Body = "Test Body" };
            var result = await _database.UpsertPrayerAsync(prayer);
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpsertPrayerAsync_NewPrayer_ShouldInsertAndReturnId()
        {
            // Arrange
            await _database.InitializeAsync();
            var prayer = new Prayer
            {
                Subject = "Test Prayer",
                Body = "Test Body",
                CategoryId = 1
            };

            // Act
            var result = await _database.UpsertPrayerAsync(prayer);

            // Assert
            result.Should().Be(1); // Number of rows affected
            prayer.Id.Should().BeGreaterThan(0); // ID should be set on the prayer object
        }

        [Fact]
        public async Task UpsertPrayerAsync_ExistingPrayer_ShouldUpdateAndReturnId()
        {
            // Arrange
            await _database.InitializeAsync();
            var prayer = new Prayer
            {
                Subject = "Test Prayer",
                Body = "Test Body"
            };
            await _database.UpsertPrayerAsync(prayer);
            var originalId = prayer.Id;
            prayer.Subject = "Updated Prayer";

            // Act
            var updateResult = await _database.UpsertPrayerAsync(prayer);

            // Assert
            updateResult.Should().Be(1); // Number of rows affected
            prayer.Id.Should().Be(originalId); // ID should remain the same
            var retrievedPrayer = await _database.GetPrayerAsync(originalId);
            retrievedPrayer.Should().NotBeNull();
            retrievedPrayer!.Subject.Should().Be("Updated Prayer");
        }

        [Fact]
        public async Task GetPrayerAsync_ExistingPrayer_ShouldReturnPrayer()
        {
            // Arrange
            await _database.InitializeAsync();
            var prayer = new Prayer
            {
                Subject = "Test Prayer",
                Body = "Test Body"
            };
            await _database.UpsertPrayerAsync(prayer);

            // Act
            var result = await _database.GetPrayerAsync(prayer.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(prayer.Id);
            result.Subject.Should().Be("Test Prayer");
            result.Body.Should().Be("Test Body");
        }

        [Fact]
        public async Task GetPrayerAsync_NonExistentPrayer_ShouldReturnNull()
        {
            // Arrange
            await _database.InitializeAsync();

            // Act
            var result = await _database.GetPrayerAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeletePrayerAsync_ShouldMarkAsDeleted()
        {
            // Arrange
            await _database.InitializeAsync();
            var prayer = new Prayer
            {
                Subject = "Test Prayer",
                Body = "Test Body"
            };
            await _database.UpsertPrayerAsync(prayer);

            // Act
            var result = await _database.DeletePrayerAsync(prayer.Id);

            // Assert
            result.Should().Be(1);
            var deletedPrayer = await _database.GetPrayerAsync(prayer.Id);
            deletedPrayer.Should().NotBeNull();
            deletedPrayer!.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task GetPrayersAsync_ShouldReturnNonDeletedPrayers()
        {
            // Arrange
            await _database.InitializeAsync();
            var prayer1 = new Prayer { Subject = "Prayer 1", Body = "Body 1" };
            var prayer2 = new Prayer { Subject = "Prayer 2", Body = "Body 2" };
            var prayer3 = new Prayer { Subject = "Prayer 3", Body = "Body 3" };

            await _database.UpsertPrayerAsync(prayer1);
            await _database.UpsertPrayerAsync(prayer2);
            await _database.UpsertPrayerAsync(prayer3);

            await _database.DeletePrayerAsync(prayer2.Id);

            // Act
            var prayers = await _database.GetPrayersAsync();

            // Assert
            prayers.Should().HaveCount(2);
            prayers.Should().Contain(p => p.Id == prayer1.Id);
            prayers.Should().Contain(p => p.Id == prayer3.Id);
            prayers.Should().NotContain(p => p.Id == prayer2.Id);
        }

        [Fact]
        public async Task GetPrayersAsync_IncludeAnsweredFalse_ShouldExcludeAnsweredPrayers()
        {
            // Arrange
            await _database.InitializeAsync();
            var prayer1 = new Prayer { Subject = "Prayer 1", Body = "Body 1" };
            var prayer2 = new Prayer { Subject = "Prayer 2", Body = "Body 2" };

            await _database.UpsertPrayerAsync(prayer1);
            await _database.UpsertPrayerAsync(prayer2);

            await _database.MarkPrayerAnsweredAsync(prayer2.Id, true);

            // Act
            var prayers = await _database.GetPrayersAsync(includeAnswered: false);

            // Assert
            prayers.Should().HaveCount(1);
            prayers.Should().Contain(p => p.Id == prayer1.Id);
            prayers.Should().NotContain(p => p.Id == prayer2.Id);
        }

        [Fact]
        public async Task GetAnsweredPrayersAsync_ShouldReturnOnlyAnsweredPrayers()
        {
            // Arrange
            await _database.InitializeAsync();
            var prayer1 = new Prayer { Subject = "Prayer 1", Body = "Body 1" };
            var prayer2 = new Prayer { Subject = "Prayer 2", Body = "Body 2" };

            await _database.UpsertPrayerAsync(prayer1);
            await _database.UpsertPrayerAsync(prayer2);

            await _database.MarkPrayerAnsweredAsync(prayer2.Id, true);

            // Act
            var answeredPrayers = await _database.GetAnsweredPrayersAsync();

            // Assert
            answeredPrayers.Should().HaveCount(1);
            answeredPrayers.Should().Contain(p => p.Id == prayer2.Id);
            answeredPrayers.Should().NotContain(p => p.Id == prayer1.Id);
        }

        [Fact]
        public async Task GetPrayersByCategoryAsync_WithCategoryId_ShouldReturnPrayersInCategory()
        {
            // Arrange
            await _database.InitializeAsync();
            var category = new Category { Name = "Test Category" };
            await _database.UpsertCategoryAsync(category);

            var prayer1 = new Prayer { Subject = "Prayer 1", Body = "Body 1", CategoryId = category.Id };
            var prayer2 = new Prayer { Subject = "Prayer 2", Body = "Body 2", CategoryId = category.Id };
            var prayer3 = new Prayer { Subject = "Prayer 3", Body = "Body 3" }; // No category

            await _database.UpsertPrayerAsync(prayer1);
            await _database.UpsertPrayerAsync(prayer2);
            await _database.UpsertPrayerAsync(prayer3);

            // Act
            var categoryPrayers = await _database.GetPrayersByCategoryAsync(category.Id);

            // Assert
            categoryPrayers.Should().HaveCount(2);
            categoryPrayers.Should().AllSatisfy(p => p.CategoryId.Should().Be(category.Id));
        }

        [Fact]
        public async Task GetPrayersByCategoryAsync_WithNullCategoryId_ShouldReturnPrayersWithoutCategory()
        {
            // Arrange
            await _database.InitializeAsync();
            var category = new Category { Name = "Test Category" };
            var categoryId = await _database.UpsertCategoryAsync(category);

            var prayer1 = new Prayer { Subject = "Prayer 1", Body = "Body 1", CategoryId = categoryId };
            var prayer2 = new Prayer { Subject = "Prayer 2", Body = "Body 2" }; // No category

            await _database.UpsertPrayerAsync(prayer1);
            await _database.UpsertPrayerAsync(prayer2);

            // Act
            var uncategorizedPrayers = await _database.GetPrayersByCategoryAsync(null);

            // Assert
            uncategorizedPrayers.Should().HaveCount(1);
            uncategorizedPrayers.Should().Contain(p => p.Id == prayer2.Id);
        }

        [Fact]
        public async Task MarkPrayerAnsweredAsync_ShouldUpdateAnsweredStatus()
        {
            // Arrange
            await _database.InitializeAsync();
            var prayer = new Prayer { Subject = "Test Prayer", Body = "Test Body" };
            await _database.UpsertPrayerAsync(prayer);

            // Act
            var result = await _database.MarkPrayerAnsweredAsync(prayer.Id, true);

            // Assert
            result.Should().Be(1);
            var updatedPrayer = await _database.GetPrayerAsync(prayer.Id);
            updatedPrayer.Should().NotBeNull();
            updatedPrayer!.IsAnswered.Should().BeTrue();
        }

        [Fact]
        public async Task UpsertCategoryAsync_NewCategory_ShouldInsertAndReturnId()
        {
            // Arrange
            await _database.InitializeAsync();
            var category = new Category { Name = "Test Category" };

            // Act
            var result = await _database.UpsertCategoryAsync(category);

            // Assert
            result.Should().Be(1); // Number of rows affected
            category.Id.Should().BeGreaterThan(0); // ID should be set on the category object
        }

        [Fact]
        public async Task UpsertCategoryAsync_ExistingCategory_ShouldUpdateAndReturnId()
        {
            // Arrange
            await _database.InitializeAsync();
            var category = new Category { Name = "Test Category" };
            await _database.UpsertCategoryAsync(category);
            var originalId = category.Id;
            category.Name = "Updated Category";

            // Act
            var updateResult = await _database.UpsertCategoryAsync(category);

            // Assert
            updateResult.Should().Be(1); // Number of rows affected
            category.Id.Should().Be(originalId); // ID should remain the same
            var categories = await _database.GetCategoriesAsync();
            var updatedCategory = categories.FirstOrDefault(c => c.Id == originalId);
            updatedCategory.Should().NotBeNull();
            updatedCategory!.Name.Should().Be("Updated Category");
        }

        [Fact]
        public async Task GetCategoriesAsync_ShouldReturnAllCategories()
        {
            // Arrange
            await _database.InitializeAsync();
            var category1 = new Category { Name = "Category 1" };
            var category2 = new Category { Name = "Category 2" };

            await _database.UpsertCategoryAsync(category1);
            await _database.UpsertCategoryAsync(category2);

            // Act
            var categories = await _database.GetCategoriesAsync();

            // Assert
            categories.Should().HaveCount(2);
            categories.Should().Contain(c => c.Name == "Category 1");
            categories.Should().Contain(c => c.Name == "Category 2");
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldRemoveCategory()
        {
            // Arrange
            await _database.InitializeAsync();
            var category = new Category { Name = "Test Category" };
            await _database.UpsertCategoryAsync(category);

            // Act
            var result = await _database.DeleteCategoryAsync(category.Id);

            // Assert
            result.Should().Be(1);
            var categories = await _database.GetCategoriesAsync();
            categories.Should().NotContain(c => c.Id == category.Id);
        }

        public void Dispose()
        {
            try
            {
                // Give SQLite time to release the file
                Thread.Sleep(100);
                if (File.Exists(_testDbPath))
                {
                    File.Delete(_testDbPath);
                }
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }
}
