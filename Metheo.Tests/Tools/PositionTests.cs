using Metheo.Tools;

namespace Metheo.Tests.Tools;

public class PositionTests
{
    [Fact]
    public void RetrievePosition_ValidCoordinates_ReturnsCorrectPosition()
    {
        // Arrange
        var latitude = 52.5200; // Berlin
        var longitude = 13.4050;

        // Act
        var result = Position.RetrievePosition(latitude, longitude);

        // Assert
        Assert.Equal(latitude, result.lat);
        Assert.Equal(longitude, result.lon);
    }

    [Theory]
    [InlineData(double.NaN, 13.4050)]
    [InlineData(52.5200, double.NaN)]
    [InlineData(-91, 13.4050)]
    [InlineData(52.5200, -181)]
    [InlineData(91, 13.4050)]
    [InlineData(52.5200, 181)]
    public void RetrievePosition_InvalidCoordinates_ThrowsArgumentOutOfRangeException(double latitude, double longitude)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Position.RetrievePosition(latitude, longitude));
    }
}