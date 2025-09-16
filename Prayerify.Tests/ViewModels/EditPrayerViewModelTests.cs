using FluentAssertions;
using Moq;
using Prayerify.Core.Data;
using Prayerify.Core.Models;
using Prayerify.Core.Services;
using Prayerify.Core.ViewModels;
using Xunit;

namespace Prayerify.Tests.ViewModels
{
    public class EditPrayerViewModelTests
    {
        private readonly Mock<IPrayerDatabase> _mockDatabase;
        private readonly Mock<IDialogService> _mockDialogService;
        private readonly EditPrayerViewModel _viewModel;

        public EditPrayerViewModelTests()
        {
            _mockDatabase = new Mock<IPrayerDatabase>();
            _mockDialogService = new Mock<IDialogService>();
            _viewModel = new EditPrayerViewModel(_mockDatabase.Object, _mockDialogService.Object);
        }

        [Fact]
        public void EditPrayerViewModel_Constructor_ShouldInitializeCorrectly()
        {
            // Assert
            _viewModel.Title.Should().Be("Edit Prayer");
            _viewModel.Categories.Should().NotBeNull();
            _viewModel.Id.Should().Be(0);
            _viewModel.Subject.Should().Be(string.Empty);
            _viewModel.Body.Should().Be(string.Empty);
            _viewModel.CategoryId.Should().BeNull();
            _viewModel.SelectedCategory.Should().BeNull();
        }

        [Fact]
        public void EditPrayerViewModel_ShouldInheritFromBaseViewModel()
        {
            // Assert
            _viewModel.Should().BeAssignableTo<BaseViewModel>();
        }

        [Fact]
        public async Task LoadAsync_WithNullId_ShouldSetTitleToAddPrayer()
        {
            // Arrange
            _mockDatabase.Setup(db => db.GetCategoriesAsync())
                        .ReturnsAsync(new List<Category>());

            // Act
            await _viewModel.LoadAsync(null);

            // Assert
            _viewModel.Title.Should().Be("Add Prayer");
        }

        [Fact]
        public async Task LoadAsync_WithZeroId_ShouldSetTitleToAddPrayer()
        {
            // Arrange
            _mockDatabase.Setup(db => db.GetCategoriesAsync())
                        .ReturnsAsync(new List<Category>());

            // Act
            await _viewModel.LoadAsync(0);

            // Assert
            _viewModel.Title.Should().Be("Add Prayer");
        }

        [Fact]
        public async Task LoadAsync_WithValidId_ShouldLoadPrayerData()
        {
            // Arrange
            var categories = new List<Category>
            {
                new() { Id = 1, Name = "Category 1" },
                new() { Id = 2, Name = "Category 2" }
            };

            var prayer = new Prayer
            {
                Id = 1,
                Subject = "Test Prayer",
                Body = "Test Body",
                CategoryId = 2
            };

            _mockDatabase.Setup(db => db.GetCategoriesAsync())
                        .ReturnsAsync(categories);
            _mockDatabase.Setup(db => db.GetPrayerAsync(1))
                        .ReturnsAsync(prayer);

            // Act
            await _viewModel.LoadAsync(1);

            // Assert
            _viewModel.Id.Should().Be(1);
            _viewModel.Subject.Should().Be("Test Prayer");
            _viewModel.Body.Should().Be("Test Body");
            _viewModel.CategoryId.Should().Be(2);
            _viewModel.SelectedCategory.Should().NotBeNull();
            _viewModel.SelectedCategory!.Id.Should().Be(2);
            _viewModel.Categories.Should().HaveCount(2);
        }

        [Fact]
        public async Task LoadAsync_WithNonExistentId_ShouldNotSetPrayerData()
        {
            // Arrange
            _mockDatabase.Setup(db => db.GetCategoriesAsync())
                        .ReturnsAsync(new List<Category>());
            _mockDatabase.Setup(db => db.GetPrayerAsync(999))
                        .ReturnsAsync((Prayer?)null);

            // Act
            await _viewModel.LoadAsync(999);

            // Assert
            _viewModel.Id.Should().Be(0);
            _viewModel.Subject.Should().Be(string.Empty);
            _viewModel.Body.Should().Be(string.Empty);
            _viewModel.CategoryId.Should().BeNull();
        }

        [Fact]
        public async Task LoadAsync_ShouldLoadCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new() { Id = 1, Name = "Category 1" },
                new() { Id = 2, Name = "Category 2" }
            };

            _mockDatabase.Setup(db => db.GetCategoriesAsync())
                        .ReturnsAsync(categories);

            // Act
            await _viewModel.LoadAsync(null);

            // Assert
            _viewModel.Categories.Should().HaveCount(2);
            _viewModel.Categories.Should().Contain(c => c.Name == "Category 1");
            _viewModel.Categories.Should().Contain(c => c.Name == "Category 2");
        }

        [Fact]
        public async Task SaveAsync_WithEmptySubject_ShouldShowAlertAndNotSave()
        {
            // Arrange
            _viewModel.Subject = string.Empty;
            _viewModel.Body = "Valid Body";

            _mockDialogService.Setup(ds => ds.ShowAlertAsync("Empty Subject", "The subject of the prayer cannot be left empty.", "OK"))
                            .Returns(Task.CompletedTask);

            // Act
            await _viewModel.SaveAsync();

            // Assert
            _mockDialogService.Verify(ds => ds.ShowAlertAsync("Empty Subject", "The subject of the prayer cannot be left empty.", "OK"), Times.Once);
            _mockDatabase.Verify(db => db.UpsertPrayerAsync(It.IsAny<Prayer>()), Times.Never);
        }

        [Fact]
        public async Task SaveAsync_WithEmptyBody_ShouldShowAlertAndNotSave()
        {
            // Arrange
            _viewModel.Subject = "Valid Subject";
            _viewModel.Body = string.Empty;

            _mockDialogService.Setup(ds => ds.ShowAlertAsync("Empty Body", "The body of the prayer cannot be left empty.", "OK"))
                            .Returns(Task.CompletedTask);

            // Act
            await _viewModel.SaveAsync();

            // Assert
            _mockDialogService.Verify(ds => ds.ShowAlertAsync("Empty Body", "The body of the prayer cannot be left empty.", "OK"), Times.Once);
            _mockDatabase.Verify(db => db.UpsertPrayerAsync(It.IsAny<Prayer>()), Times.Never);
        }

        [Fact]
        public async Task SaveAsync_WithValidData_ShouldSavePrayer()
        {
            // Arrange
            _viewModel.Subject = "Test Subject";
            _viewModel.Body = "Test Body";
            _viewModel.Id = 0; // New prayer

            var selectedCategory = new Category { Id = 1, Name = "Test Category" };
            _viewModel.SelectedCategory = selectedCategory;

            _mockDatabase.Setup(db => db.UpsertPrayerAsync(It.IsAny<Prayer>()))
                        .ReturnsAsync(1);

            // Act
            await _viewModel.SaveAsync();

            // Assert
            _mockDatabase.Verify(db => db.UpsertPrayerAsync(It.Is<Prayer>(p => 
                p.Subject == "Test Subject" && 
                p.Body == "Test Body" && 
                p.CategoryId == 1 &&
                p.Id == 0)), Times.Once);
        }

        [Fact]
        public async Task SaveAsync_WithExistingPrayer_ShouldUpdatePrayer()
        {
            // Arrange
            _viewModel.Id = 1;
            _viewModel.Subject = "Updated Subject";
            _viewModel.Body = "Updated Body";
            _viewModel.SelectedCategory = null;

            _mockDatabase.Setup(db => db.UpsertPrayerAsync(It.IsAny<Prayer>()))
                        .ReturnsAsync(1);

            // Act
            await _viewModel.SaveAsync();

            // Assert
            _mockDatabase.Verify(db => db.UpsertPrayerAsync(It.Is<Prayer>(p => 
                p.Id == 1 && 
                p.Subject == "Updated Subject" && 
                p.Body == "Updated Body" && 
                p.CategoryId == null)), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Valid Subject")]
        [InlineData("A very long subject that might contain special characters like @#$%^&*()")]
        public void Subject_ShouldSetAndGetCorrectly(string subject)
        {
            // Act
            _viewModel.Subject = subject;

            // Assert
            _viewModel.Subject.Should().Be(subject);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Valid Body")]
        [InlineData("A very long prayer body that might contain multiple lines and special characters like @#$%^&*()")]
        public void Body_ShouldSetAndGetCorrectly(string body)
        {
            // Act
            _viewModel.Body = body;

            // Assert
            _viewModel.Body.Should().Be(body);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(1)]
        [InlineData(5)]
        public void CategoryId_ShouldSetAndGetCorrectly(int? categoryId)
        {
            // Act
            _viewModel.CategoryId = categoryId;

            // Assert
            _viewModel.CategoryId.Should().Be(categoryId);
        }

        [Fact]
        public void SelectedCategory_ShouldSetAndGetCorrectly()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };

            // Act
            _viewModel.SelectedCategory = category;

            // Assert
            _viewModel.SelectedCategory.Should().Be(category);
        }
    }
}
