using FluentAssertions;
using Moq;
using Prayerify.Core.Data;
using Prayerify.Core.Models;
using Prayerify.Core.ViewModels;
using Prayerify.Core.Services;
using Xunit;

namespace Prayerify.Tests.Integration
{
    public class PrayerWorkflowTests : IDisposable
    {
        private readonly PrayerDatabase _database;
        private readonly string _testDbPath;

        public PrayerWorkflowTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"integration_test_prayerify_{Guid.NewGuid()}.db3");
            _database = new PrayerDatabase(_testDbPath);
        }

        [Fact]
        public async Task CompletePrayerWorkflow_ShouldWorkEndToEnd()
        {
            // Arrange
            await _database.InitializeAsync();
            var mockDialogService = new Mock<IDialogService>();
            var prayersViewModel = new PrayersViewModel(_database);
            var editPrayerViewModel = new EditPrayerViewModel(_database, mockDialogService.Object);
            var categoriesViewModel = new CategoriesViewModel(_database, mockDialogService.Object);

            // Act & Assert - Step 1: Create a category
            await categoriesViewModel.LoadAsync();
            categoriesViewModel.NewCategoryName = "Test Category";
            await categoriesViewModel.AddCategoryAsync();

            categoriesViewModel.Categories.Should().HaveCount(1);
            var createdCategory = categoriesViewModel.Categories.First();
            createdCategory.Name.Should().Be("Test Category");

            // Step 2: Create a prayer
            await editPrayerViewModel.LoadAsync(null);
            editPrayerViewModel.Subject = "Test Prayer";
            editPrayerViewModel.Body = "This is a test prayer body";
            editPrayerViewModel.SelectedCategory = createdCategory;
            await editPrayerViewModel.SaveAsync();

            // Step 3: Load prayers and verify
            await prayersViewModel.LoadAsync();
            prayersViewModel.Prayers.Should().HaveCount(1);
            var createdPrayer = prayersViewModel.Prayers.First();
            createdPrayer.Subject.Should().Be("Test Prayer");
            createdPrayer.Body.Should().Be("This is a test prayer body");
            createdPrayer.CategoryId.Should().Be(createdCategory.Id);

            // Step 4: Mark prayer as answered
            await prayersViewModel.ToggleAnsweredAsync(createdPrayer);
            createdPrayer.IsAnswered.Should().BeTrue();
            prayersViewModel.Prayers.Should().BeEmpty(); // Should be removed from active prayers

            // Step 5: Verify answered prayers
            var answeredPrayers = await _database.GetAnsweredPrayersAsync();
            answeredPrayers.Should().HaveCount(1);
            answeredPrayers.First().Subject.Should().Be("Test Prayer");
        }

        [Fact]
        public async Task PrayerWithCategoryWorkflow_ShouldMaintainRelationships()
        {
            // Arrange
            await _database.InitializeAsync();
            var mockDialogService = new Mock<IDialogService>();
            var categoriesViewModel = new CategoriesViewModel(_database, mockDialogService.Object);
            var editPrayerViewModel = new EditPrayerViewModel(_database, mockDialogService.Object);

            // Act - Create multiple categories
            await categoriesViewModel.LoadAsync();
            categoriesViewModel.NewCategoryName = "Health";
            await categoriesViewModel.AddCategoryAsync();
            categoriesViewModel.NewCategoryName = "Family";
            await categoriesViewModel.AddCategoryAsync();

            var healthCategory = categoriesViewModel.Categories.First(c => c.Name == "Health");
            var familyCategory = categoriesViewModel.Categories.First(c => c.Name == "Family");

            // Create prayers in different categories
            await editPrayerViewModel.LoadAsync(null);
            editPrayerViewModel.Subject = "Health Prayer";
            editPrayerViewModel.Body = "Prayer for good health";
            editPrayerViewModel.SelectedCategory = healthCategory;
            await editPrayerViewModel.SaveAsync();

            await editPrayerViewModel.LoadAsync(null);
            editPrayerViewModel.Subject = "Family Prayer";
            editPrayerViewModel.Body = "Prayer for family";
            editPrayerViewModel.SelectedCategory = familyCategory;
            await editPrayerViewModel.SaveAsync();

            // Assert - Verify category relationships
            var healthPrayers = await _database.GetPrayersByCategoryAsync(healthCategory.Id);
            var familyPrayers = await _database.GetPrayersByCategoryAsync(familyCategory.Id);
            var uncategorizedPrayers = await _database.GetPrayersByCategoryAsync(null);

            healthPrayers.Should().HaveCount(1);
            healthPrayers.First().Subject.Should().Be("Health Prayer");

            familyPrayers.Should().HaveCount(1);
            familyPrayers.First().Subject.Should().Be("Family Prayer");

            uncategorizedPrayers.Should().BeEmpty();
        }

        [Fact]
        public async Task PrayerEditWorkflow_ShouldUpdateCorrectly()
        {
            // Arrange
            await _database.InitializeAsync();
            var mockDialogService = new Mock<IDialogService>();
            var editPrayerViewModel = new EditPrayerViewModel(_database, mockDialogService.Object);

            // Act - Create initial prayer
            await editPrayerViewModel.LoadAsync(null);
            editPrayerViewModel.Subject = "Original Subject";
            editPrayerViewModel.Body = "Original Body";
            await editPrayerViewModel.SaveAsync();

            var createdPrayer = await _database.GetPrayersAsync();
            createdPrayer.Should().HaveCount(1);
            var prayerId = createdPrayer.First().Id;

            // Edit the prayer
            await editPrayerViewModel.LoadAsync(prayerId);
            editPrayerViewModel.Subject = "Updated Subject";
            editPrayerViewModel.Body = "Updated Body";
            await editPrayerViewModel.SaveAsync();

            // Assert
            var updatedPrayer = await _database.GetPrayerAsync(prayerId);
            updatedPrayer.Should().NotBeNull();
            updatedPrayer!.Subject.Should().Be("Updated Subject");
            updatedPrayer.Body.Should().Be("Updated Body");
            updatedPrayer.Id.Should().Be(prayerId); // Same ID
        }

        [Fact]
        public async Task CategoryDeletionWorkflow_ShouldHandleOrphanedPrayers()
        {
            // Arrange
            await _database.InitializeAsync();
            var mockDialogService = new Mock<IDialogService>();
            var categoriesViewModel = new CategoriesViewModel(_database, mockDialogService.Object);
            var editPrayerViewModel = new EditPrayerViewModel(_database, mockDialogService.Object);

            // Act - Create category and prayer
            await categoriesViewModel.LoadAsync();
            categoriesViewModel.NewCategoryName = "Test Category";
            await categoriesViewModel.AddCategoryAsync();

            var category = categoriesViewModel.Categories.First();
            await editPrayerViewModel.LoadAsync(null);
            editPrayerViewModel.Subject = "Test Prayer";
            editPrayerViewModel.Body = "Test Body";
            editPrayerViewModel.SelectedCategory = category;
            await editPrayerViewModel.SaveAsync();

            // Delete category
            await categoriesViewModel.DeleteCategoryAsync(category);

            // Assert
            categoriesViewModel.Categories.Should().BeEmpty();
            var prayers = await _database.GetPrayersAsync();
            prayers.Should().HaveCount(1);
            prayers.First().CategoryId.Should().Be(category.Id); // Prayer still references deleted category
        }

        [Fact]
        public async Task PrayerDeletionWorkflow_ShouldSoftDelete()
        {
            // Arrange
            await _database.InitializeAsync();
            var mockDialogService = new Mock<IDialogService>();
            var prayersViewModel = new PrayersViewModel(_database);
            var editPrayerViewModel = new EditPrayerViewModel(_database, mockDialogService.Object);

            // Act - Create prayer
            await editPrayerViewModel.LoadAsync(null);
            editPrayerViewModel.Subject = "Test Prayer";
            editPrayerViewModel.Body = "Test Body";
            await editPrayerViewModel.SaveAsync();

            await prayersViewModel.LoadAsync();
            var prayer = prayersViewModel.Prayers.First();

            // Delete prayer
            await prayersViewModel.DeletePrayerAsync(prayer);

            // Assert
            prayersViewModel.Prayers.Should().BeEmpty();
            var allPrayers = await _database.GetPrayersAsync(includeDeleted: true);
            allPrayers.Should().HaveCount(1);
            allPrayers.First().IsDeleted.Should().BeTrue();
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
