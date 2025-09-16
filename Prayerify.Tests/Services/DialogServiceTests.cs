using FluentAssertions;
using Moq;
using Prayerify.Core.Services;
using Xunit;

namespace Prayerify.Tests.Services
{
    public class DialogServiceTests
    {
        [Fact]
        public void DialogService_ImplementsIDialogService()
        {
            // Arrange & Act
            var dialogService = new DialogService();

            // Assert
            dialogService.Should().BeAssignableTo<IDialogService>();
        }

        [Fact]
        public void IDialogService_ShouldHaveCorrectMethodSignature()
        {
            // Arrange
            var interfaceType = typeof(IDialogService);
            var method = interfaceType.GetMethod("ShowAlertAsync");

            // Assert
            method.Should().NotBeNull();
            method!.ReturnType.Should().Be(typeof(Task));
            
            var parameters = method.GetParameters();
            parameters.Should().HaveCount(3);
            parameters[0].ParameterType.Should().Be(typeof(string));
            parameters[0].Name.Should().Be("title");
            parameters[1].ParameterType.Should().Be(typeof(string));
            parameters[1].Name.Should().Be("message");
            parameters[2].ParameterType.Should().Be(typeof(string));
            parameters[2].Name.Should().Be("cancel");
        }

        [Fact]
        public void IDialogService_ShowAlertAsync_ShouldHaveDefaultParameter()
        {
            // Arrange
            var interfaceType = typeof(IDialogService);
            var method = interfaceType.GetMethod("ShowAlertAsync");
            var parameters = method!.GetParameters();

            // Assert
            parameters[2].HasDefaultValue.Should().BeTrue();
            parameters[2].DefaultValue.Should().Be("OK");
        }

        // Note: Testing the actual DialogService.ShowAlertAsync method is challenging
        // because it depends on Shell.Current and Application.Current which are
        // part of the MAUI framework and not easily testable in a unit test environment.
        // In a real-world scenario, you might want to:
        // 1. Create a mock implementation for testing
        // 2. Use integration tests to test the actual dialog functionality
        // 3. Refactor the service to be more testable by injecting dependencies

        [Fact]
        public void DialogService_Constructor_ShouldNotThrow()
        {
            // Act & Assert
            var action = () => new DialogService();
            action.Should().NotThrow();
        }
    }
}
