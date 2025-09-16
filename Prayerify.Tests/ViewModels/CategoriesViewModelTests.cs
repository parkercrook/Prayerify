using FluentAssertions;
using Moq;
using Prayerify.Core.Data;
using Prayerify.Core.Models;
using Prayerify.Core.Services;
using Prayerify.Core.ViewModels;
using Xunit;

namespace Prayerify.Tests.ViewModels
{
    public class CategoriesViewModelTests
    {
        private readonly Mock<IPrayerDatabase> _mockDatabase;
        private readonly Mock<IDialogService> _mockDialogService;
        private readonly CategoriesViewModel _viewModel;

        public CategoriesViewModelTests()
        {
            _mockDatabase = new Mock<IPrayerDatabase>();
            _mockDialogService = new Mock<IDialogService>();
            _viewModel = new CategoriesViewModel(_mockDatabase.Object, _mockDialogService.Object);
        }

        [Fact]
        public void CategoriesViewModel_Constructor_ShouldInitializeCorrectly()
        {
            // Assert
            _viewModel.Title.Should().Be("Categories");
            _viewModel.Categories.Should().NotBeNull();
            _viewModel.NewCategoryName.Should().Be(string.Empty);
        }

        [Fact]
        public void CategoriesViewModel_ShouldInheritFromBaseViewModel()
        {
            // Assert
            _viewModel.Should().BeAssignableTo<BaseViewModel>();
        }

        [Fact]
        public async Task LoadAsync_WhenNotBusy_ShouldLoadCategories()
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
            await _viewModel.LoadAsync();

            // Assert
            _viewModel.Categories.Should().HaveCount(2);
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
            _mockDatabase.Verify(db => db.GetCategoriesAsync(), Times.Never);
        }

        [Fact]
        public async Task LoadAsync_ShouldSetIsBusyCorrectly()
        {
            // Arrange
            _mockDatabase.Setup(db => db.GetCategoriesAsync())
                        .ReturnsAsync(new List<Category>());

            // Act
            await _viewModel.LoadAsync();

            // Assert
            _viewModel.IsBusy.Should().BeFalse();
        }

        [Fact]
        public async Task LoadAsync_ShouldClearExistingCategories()
        {
            // Arrange
            _viewModel.Categories.Add(new Category { Id = 999, Name = "Old Category" });

            _mockDatabase.Setup(db => db.GetCategoriesAsync())
                        .ReturnsAsync(new List<Category>());

            // Act
            await _viewModel.LoadAsync();

            // Assert
            _viewModel.Categories.Should().NotContain(c => c.Id == 999);
        }

        [Fact]
        public async Task AddCategoryAsync_WithEmptyName_ShouldShowAlertAndNotAdd()
        {
            // Arrange
            _viewModel.NewCategoryName = string.Empty;

            _mockDialogService.Setup(ds => ds.ShowAlertAsync("Empty Category Name", "The category name cannot be left empty.", "OK"))
                            .Returns(Task.CompletedTask);

            // Act
            await _viewModel.AddCategoryAsync();

            // Assert
            _mockDialogService.Verify(ds => ds.ShowAlertAsync("Empty Category Name", "The category name cannot be left empty.", "OK"), Times.Once);
            _mockDatabase.Verify(db => db.UpsertCategoryAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task AddCategoryAsync_WithWhitespaceName_ShouldNotAdd()
        {
            // Arrange
            _viewModel.NewCategoryName = "   ";

            // Act
            await _viewModel.AddCategoryAsync();

            // Assert
            _mockDatabase.Verify(db => db.UpsertCategoryAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task AddCategoryAsync_WithValidName_ShouldAddCategory()
        {
            // Arrange
            _viewModel.NewCategoryName = "Test Category";

            var addedCategory = new Category { Id = 1, Name = "Test Category" };
            _mockDatabase.Setup(db => db.UpsertCategoryAsync(It.IsAny<Category>()))
                        .Callback<Category>(c => c.Id = 1)
                        .ReturnsAsync(1);

            // Act
            await _viewModel.AddCategoryAsync();

            // Assert
            _mockDatabase.Verify(db => db.UpsertCategoryAsync(It.Is<Category>(c => c.Name == "Test Category")), Times.Once);
            _viewModel.Categories.Should().HaveCount(1);
            _viewModel.Categories.Should().Contain(c => c.Name == "Test Category");
            _viewModel.NewCategoryName.Should().Be(string.Empty);
        }

        [Fact]
        public async Task AddCategoryAsync_WithTrimmedName_ShouldAddCategory()
        {
            // Arrange
            _viewModel.NewCategoryName = "  Test Category  ";

            _mockDatabase.Setup(db => db.UpsertCategoryAsync(It.IsAny<Category>()))
                        .Callback<Category>(c => c.Id = 1)
                        .ReturnsAsync(1);

            // Act
            await _viewModel.AddCategoryAsync();

            // Assert
            _mockDatabase.Verify(db => db.UpsertCategoryAsync(It.Is<Category>(c => c.Name == "Test Category")), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WithValidCategory_ShouldDeleteAndRemoveFromCollection()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };
            _viewModel.Categories.Add(category);

            _mockDatabase.Setup(db => db.DeleteCategoryAsync(1))
                        .ReturnsAsync(1);

            // Act
            await _viewModel.DeleteCategoryAsync(category);

            // Assert
            _mockDatabase.Verify(db => db.DeleteCategoryAsync(1), Times.Once);
            _viewModel.Categories.Should().NotContain(category);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WithNullCategory_ShouldNotDelete()
        {
            // Act
            await _viewModel.DeleteCategoryAsync(null);

            // Assert
            _mockDatabase.Verify(db => db.DeleteCategoryAsync(It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Valid Category Name")]
        [InlineData("A very long category name that might contain special characters like @#$%^&*()")]
        public void NewCategoryName_ShouldSetAndGetCorrectly(string name)
        {
            // Act
            _viewModel.NewCategoryName = name;

            // Assert
            _viewModel.NewCategoryName.Should().Be(name);
        }
    }
}
