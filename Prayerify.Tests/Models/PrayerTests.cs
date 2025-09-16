using FluentAssertions;
using Prayerify.Core.Models;
using Xunit;

namespace Prayerify.Tests.Models
{
    public class PrayerTests
    {
        [Fact]
        public void Prayer_DefaultConstructor_ShouldInitializeWithDefaultValues()
        {
            // Act
            var prayer = new Prayer();

            // Assert
            prayer.Id.Should().Be(0);
            prayer.CategoryId.Should().BeNull();
            prayer.Subject.Should().Be(string.Empty);
            prayer.Body.Should().Be(string.Empty);
            prayer.IsAnswered.Should().BeFalse();
            prayer.IsDeleted.Should().BeFalse();
            prayer.CreatedUtc.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Prayer_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var categoryId = 1;
            var subject = "Test Prayer";
            var body = "This is a test prayer body";
            var isAnswered = true;
            var isDeleted = false;
            var createdUtc = "January 01, 2024";

            // Act
            var prayer = new Prayer
            {
                Id = 1,
                CategoryId = categoryId,
                Subject = subject,
                Body = body,
                IsAnswered = isAnswered,
                IsDeleted = isDeleted,
                CreatedUtc = createdUtc
            };

            // Assert
            prayer.Id.Should().Be(1);
            prayer.CategoryId.Should().Be(categoryId);
            prayer.Subject.Should().Be(subject);
            prayer.Body.Should().Be(body);
            prayer.IsAnswered.Should().Be(isAnswered);
            prayer.IsDeleted.Should().Be(isDeleted);
            prayer.CreatedUtc.Should().Be(createdUtc);
        }

        [Fact]
        public void Prayer_WithNullCategoryId_ShouldBeValid()
        {
            // Act
            var prayer = new Prayer
            {
                CategoryId = null
            };

            // Assert
            prayer.CategoryId.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Valid Subject")]
        [InlineData("A very long subject that might contain special characters like @#$%^&*()")]
        public void Prayer_WithVariousSubjects_ShouldAcceptAllValues(string subject)
        {
            // Act
            var prayer = new Prayer
            {
                Subject = subject
            };

            // Assert
            prayer.Subject.Should().Be(subject);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Valid Body")]
        [InlineData("A very long prayer body that might contain multiple lines and special characters like @#$%^&*()")]
        public void Prayer_WithVariousBodies_ShouldAcceptAllValues(string body)
        {
            // Act
            var prayer = new Prayer
            {
                Body = body
            };

            // Assert
            prayer.Body.Should().Be(body);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void Prayer_WithVariousBooleanValues_ShouldSetCorrectly(bool isAnswered, bool isDeleted)
        {
            // Act
            var prayer = new Prayer
            {
                IsAnswered = isAnswered,
                IsDeleted = isDeleted
            };

            // Assert
            prayer.IsAnswered.Should().Be(isAnswered);
            prayer.IsDeleted.Should().Be(isDeleted);
        }
    }
}
