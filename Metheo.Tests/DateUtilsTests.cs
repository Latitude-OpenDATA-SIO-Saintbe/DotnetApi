using System;
using Xunit;
using Metheo.Tools; // Ensure this matches the namespace of DateUtils

namespace Metheo.Tests
{
    public class DateUtilsTests
    {
        [Theory]
        [InlineData("2024", "2024-01-01", null)]
        [InlineData("03-2024", "2024-03-01", null)]
        [InlineData("25-03-2024", "2024-03-25", null)]
        
        [InlineData("2025:2026", "2025-01-01", "2026-12-31")]
        [InlineData("03-2025:02-2026", "2025-03-01", "2026-02-28")]
        [InlineData("01-02-2024:08-03-2024", "2024-02-01", "2024-03-08")]
        
        [InlineData("01-02-2024:05-2024", "2024-02-01", "2024-05-31")]
        [InlineData("01-02-2024:2024", "2024-02-01", "2024-12-31")]
        
        [InlineData("03-2024:2024", "2024-03-01", "2024-12-31")]
        [InlineData("03-2024:05-2024", "2024-03-01", "2024-05-31")]
        
        [InlineData("2024:01-03-2024", "2024-01-01", "2024-03-01")]
        [InlineData("02-2024:01-03-2024", "2024-02-01", "2024-03-01")]
        [InlineData("2024:03-2024", "2024-01-01", "2024-03-31")]
        public void GetDateRange_ValidInput_ReturnsExpectedDates(string input, string expectedStart, string expectedEnd)
        {
            // Act
            var result = DateUtils.GetDateRange(input);

            // Assert
            Assert.Equal(DateTime.Parse(expectedStart), result.startDate);
            if (expectedEnd != null)
            {
                Assert.Equal(DateTime.Parse(expectedEnd), result.endDate);
            }
            else
            {
                Assert.Null(result.endDate);
            }
        }

        [Theory]
        [InlineData("invalid-date")]
        public void GetDateRange_InvalidInput_ThrowsException(string input)
        {
            // Act & Assert
            var exception = Assert.Throws<FormatException>(() => DateUtils.GetDateRange(input));
            Assert.Equal("Invalid date format", exception.Message);
        }
    }
}