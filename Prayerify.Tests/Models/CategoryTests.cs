using FluentAssertions;
using Prayerify.Core.Models;
using Xunit;

namespace Prayerify.Tests.Models
{
    public class CategoryTests
    {
        [Fact]
        public void Category_DefaultConstructor_ShouldInitializeWithDefaultValues()
        {
            // Act
            var category = new Category();

            // Assert
            category.Id.Should().Be(0);
            category.Name.Should().Be(string.Empty);
            category.CreatedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Category_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = 1;
            var name = "Test Category";
            var createdUtc = DateTime.UtcNow.AddDays(-1);

            // Act
            var category = new Category
            {
                Id = id,
                Name = name,
                CreatedUtc = createdUtc
            };

            // Assert
            category.Id.Should().Be(id);
            category.Name.Should().Be(name);
            category.CreatedUtc.Should().Be(createdUtc);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Valid Category")]
        [InlineData("A very long category name that might contain special characters like @#$%^&*()")]
        [InlineData("Category with numbers 123")]
        public void Category_WithVariousNames_ShouldAcceptAllValues(string name)
        {
            // Act
            var category = new Category
            {
                Name = name
            };

            // Assert
            category.Name.Should().Be(name);
        }

        [Fact]
        public void Category_CreatedUtc_ShouldDefaultToCurrentUtcTime()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;

            // Act
            var category = new Category();

            // Assert
            var afterCreation = DateTime.UtcNow;
            category.CreatedUtc.Should().BeOnOrAfter(beforeCreation);
            category.CreatedUtc.Should().BeOnOrBefore(afterCreation);
        }

        [Fact]
        public void Category_WithCustomCreatedUtc_ShouldSetCorrectly()
        {
            // Arrange
            var customDate = new DateTime(2023, 12, 25, 10, 30, 0, DateTimeKind.Utc);

            // Act
            var category = new Category
            {
                CreatedUtc = customDate
            };

            // Assert
            category.CreatedUtc.Should().Be(customDate);
        }
    }
}
