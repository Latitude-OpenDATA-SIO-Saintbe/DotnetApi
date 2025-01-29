using Metheo.Tools;

namespace Metheo.Tests.Tools;

public class DateUtilsTests
{
    [Theory]
    [InlineData("2023", "2023-01-01", "2023-12-31")]
    [InlineData("01-2023", "2023-01-01", "2023-01-31")]
    [InlineData("01-01-2023", "2023-01-01", "2023-01-01")]
    [InlineData("2023:2024", "2023-01-01", "2024-12-31")]
    [InlineData("01-2023:02-2023", "2023-01-01", "2023-02-28")]
    [InlineData("01-01-2023:31-01-2023", "2023-01-01", "2023-01-31")]
    [InlineData("01-01-2023:2024", "2023-01-01", "2024-12-31")]
    [InlineData("2023:31-01-2024", "2023-01-01", "2024-01-31")]
    [InlineData("01-2023:31-01-2024", "2023-01-01", "2024-01-31")]
    [InlineData("01-01-2023:01-2024", "2023-01-01", "2024-01-31")]
    [InlineData("2023:01-2024", "2023-01-01", "2024-01-31")]
    [InlineData("01-2023:2024", "2023-01-01", "2024-12-31")]
    [InlineData("12-2022:2023", "2022-12-01", "2023-12-31")]
    public void TryParseDateRange_ValidDateRange_ReturnsTrue(string dateRange, string expectedStart, string expectedEnd)
    {
        // Act
        var result = DateUtils.TryParseDateRange(dateRange, out var startDate, out var endDate);

        // Assert
        Assert.True(result);
        Assert.Equal(DateTime.Parse(expectedStart), startDate);
        Assert.Equal(DateTime.Parse(expectedEnd), endDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("2023-13")]
    [InlineData("32-01-2023")]
    [InlineData("01-2023:32-01-2023")]
    public void TryParseDateRange_InvalidDateRange_ReturnsFalse(string dateRange)
    {
        // Act
        var result = DateUtils.TryParseDateRange(dateRange, out var startDate, out var endDate);

        // Assert
        Assert.False(result);
        Assert.Equal(default, startDate);
        Assert.Null(endDate);
    }
}