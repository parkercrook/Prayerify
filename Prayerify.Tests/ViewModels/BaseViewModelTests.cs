using FluentAssertions;
using Prayerify.Core.ViewModels;
using Xunit;

namespace Prayerify.Tests.ViewModels
{
    public class BaseViewModelTests
    {
        [Fact]
        public void BaseViewModel_DefaultConstructor_ShouldInitializeWithDefaultValues()
        {
            // Act
            var viewModel = new BaseViewModel();

            // Assert
            viewModel.IsBusy.Should().BeFalse();
            viewModel.Title.Should().Be(string.Empty);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BaseViewModel_IsBusy_ShouldSetAndGetCorrectly(bool isBusy)
        {
            // Arrange
            var viewModel = new BaseViewModel();

            // Act
            viewModel.IsBusy = isBusy;

            // Assert
            viewModel.IsBusy.Should().Be(isBusy);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Test Title")]
        [InlineData("A very long title that might contain special characters like @#$%^&*()")]
        public void BaseViewModel_Title_ShouldSetAndGetCorrectly(string title)
        {
            // Arrange
            var viewModel = new BaseViewModel();

            // Act
            viewModel.Title = title;

            // Assert
            viewModel.Title.Should().Be(title);
        }

        [Fact]
        public void BaseViewModel_ShouldImplementObservableObject()
        {
            // Act
            var viewModel = new BaseViewModel();

            // Assert
            viewModel.Should().BeAssignableTo<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>();
        }
    }
}
