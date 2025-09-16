using FluentAssertions;
using Moq;
using Prayerify.Core.Data;
using Prayerify.Core.Models;
using Prayerify.Core.ViewModels;
using Xunit;

namespace Prayerify.Tests.ViewModels
{
    public class PrayersViewModelTests
    {
        private readonly Mock<IPrayerDatabase> _mockDatabase;
        private readonly PrayersViewModel _viewModel;

        public PrayersViewModelTests()
        {
            _mockDatabase = new Mock<IPrayerDatabase>();
            _viewModel = new PrayersViewModel(_mockDatabase.Object);
        }

        [Fact]
        public void PrayersViewModel_Constructor_ShouldInitializeCorrectly()
        {
            // Assert
            _viewModel.Title.Should().Be("Prayers");
            _viewModel.Prayers.Should().NotBeNull();
            _viewModel.Categories.Should().NotBeNull();
            _viewModel.SessionCount.Should().Be(1);
        }

        [Fact]
        public void PrayersViewModel_ShouldInheritFromBaseViewModel()
        {
            // Assert
            _viewModel.Should().BeAssignableTo<BaseViewModel>();
        }

        [Fact]
        public async Task LoadAsync_WhenNotBusy_ShouldLoadPrayersAndCategories()
        {
            // Arrange
            var prayers = new List<Prayer>
            {
                new() { Id = 1, Subject = "Prayer 1", Body = "Body 1" },
                new() { Id = 2, Subject = "Prayer 2", Body = "Body 2" }
            };

            var categories = new List<Category>
            {
                new() { Id = 1, Name = "Category 1" },
                new() { Id = 2, Name = "Category 2" }
            };

            _mockDatabase.Setup(db => db.GetPrayersAsync(false, false))
                        .ReturnsAsync(prayers);
            _mockDatabase.Setup(db => db.GetCategoriesAsync())
                        .ReturnsAsync(categories);

            // Act
            await _viewModel.LoadAsync();

            // Assert
            _viewModel.Prayers.Should().HaveCount(2);
            _viewModel.Categories.Should().HaveCount(2);
            _viewModel.Prayers.Should().Contain(p => p.Subject == "Prayer 1");
            _viewModel.Prayers.Should().Contain(p => p.Subject == "Prayer 2");
            _viewModel.Categories.Should().Contain(c => c.Name == "Category 1");
            _viewModel.Categories.Should().Contain(c => c.Name == "Category 2");
        }

        [Fact]
        public async Task LoadAsync_WhenBusy_ShouldNotLoad()
        {
            // Arrange
            _viewModel.IsBusy = true;

            // Act
            await _viewModel.LoadAsync();

            // Assert
            _mockDatabase.Verify(db => db.GetPrayersAsync(It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
            _mockDatabase.Verify(db => db.GetCategoriesAsync(), Times.Never);
        }

        [Fact]
        public async Task LoadAsync_ShouldSetIsBusyCorrectly()
        {
            // Arrange
            _mockDatabase.Setup(db => db.GetPrayersAsync(It.IsAny<bool>(), It.IsAny<bool>()))
                        .ReturnsAsync(new List<Prayer>());
            _mockDatabase.Setup(db => db.GetCategoriesAsync())
                        .ReturnsAsync(new List<Category>());

            // Act
            await _viewModel.LoadAsync();

            // Assert
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task LoadAsync_ShouldClearExistingCollections()
        {
            // Arrange
            _viewModel.Prayers.Add(new Prayer { Id = 999, Subject = "Old Prayer" });
            _viewModel.Categories.Add(new Category { Id = 999, Name = "Old Category" });

            _mockDatabase.Setup(db => db.GetPrayersAsync(It.IsAny<bool>(), It.IsAny<bool>()))
                        .ReturnsAsync(new List<Prayer>());
            _mockDatabase.Setup(db => db.GetCategoriesAsync())
                        .ReturnsAsync(new List<Category>());

            // Act
            await _viewModel.LoadAsync();

            // Assert
            _viewModel.Prayers.Should().NotContain(p => p.Id == 999);
            _viewModel.Categories.Should().NotContain(c => c.Id == 999);
        }

        [Fact]
        public async Task DeletePrayerAsync_WithValidPrayer_ShouldDeleteAndRemoveFromCollection()
        {
            // Arrange
            var prayer = new Prayer { Id = 1, Subject = "Test Prayer", Body = "Test Body" };
            _viewModel.Prayers.Add(prayer);

            _mockDatabase.Setup(db => db.DeletePrayerAsync(1))
                        .ReturnsAsync(1);

            // Act
            await _viewModel.DeletePrayerAsync(prayer);

            // Assert
            _mockDatabase.Verify(db => db.DeletePrayerAsync(1), Times.Once);
            _viewModel.Prayers.Should().NotContain(prayer);
        }

        [Fact]
        public async Task DeletePrayerAsync_WithNullPrayer_ShouldNotDelete()
        {
            // Act
            await _viewModel.DeletePrayerAsync(null);

            // Assert
            _mockDatabase.Verify(db => db.DeletePrayerAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ToggleAnsweredAsync_WithValidPrayer_ShouldToggleStatus()
        {
            // Arrange
            var prayer = new Prayer { Id = 1, Subject = "Test Prayer", Body = "Test Body", IsAnswered = false };
            _viewModel.Prayers.Add(prayer);

            _mockDatabase.Setup(db => db.MarkPrayerAnsweredAsync(1, true))
                        .ReturnsAsync(1);

            // Act
            await _viewModel.ToggleAnsweredAsync(prayer);

            // Assert
            prayer.IsAnswered.Should().BeTrue();
            _mockDatabase.Verify(db => db.MarkPrayerAnsweredAsync(1, true), Times.Once);
            _viewModel.Prayers.Should().NotContain(prayer); // Should be removed when answered
        }

        [Fact]
        public async Task ToggleAnsweredAsync_WithNullPrayer_ShouldNotToggle()
        {
            // Act
            await _viewModel.ToggleAnsweredAsync(null);

            // Assert
            _mockDatabase.Verify(db => db.MarkPrayerAnsweredAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task ToggleAnsweredAsync_WhenPrayerBecomesUnanswered_ShouldNotRemoveFromCollection()
        {
            // Arrange
            var prayer = new Prayer { Id = 1, Subject = "Test Prayer", Body = "Test Body", IsAnswered = true };
            _viewModel.Prayers.Add(prayer);

            _mockDatabase.Setup(db => db.MarkPrayerAnsweredAsync(1, false))
                        .ReturnsAsync(1);

            // Act
            await _viewModel.ToggleAnsweredAsync(prayer);

            // Assert
            prayer.IsAnswered.Should().BeFalse();
            _mockDatabase.Verify(db => db.MarkPrayerAnsweredAsync(1, false), Times.Once);
            _viewModel.Prayers.Should().Contain(prayer); // Should remain in collection
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void SessionCount_ShouldSetAndGetCorrectly(int sessionCount)
        {
            // Act
            _viewModel.SessionCount = sessionCount;

            // Assert
            _viewModel.SessionCount.Should().Be(sessionCount);
        }
    }
}
