using Metheo.Tools;

namespace Metheo.Tests.Tools;

public class CalculTests
{
    [Fact]
    public void CalculateHaversineDistance_ReturnsCorrectDistance()
    {
        // Arrange
        var lat1 = 52.5200; // Berlin
        var lon1 = 13.4050;
        var lat2 = 48.8566; // Paris
        var lon2 = 2.3522;

        // Act
        var distance = Calcul.CalculateHaversineDistance(lat1, lon1, lat2, lon2);

        // Assert
        Assert.InRange(distance, 877, 880); // The distance between Berlin and Paris is approximately 878 km
    }

    [Fact]
    public void CalculateHaversineDistance_SameLocation_ReturnsZero()
    {
        // Arrange
        var lat = 52.5200; // Berlin
        var lon = 13.4050;

        // Act
        var distance = Calcul.CalculateHaversineDistance(lat, lon, lat, lon);

        // Assert
        Assert.Equal(0, distance);
    }

    [Fact]
    public void CalculateHaversineDistance_NegativeCoordinates_ReturnsCorrectDistance()
    {
        // Arrange
        var lat1 = -33.8688; // Sydney
        var lon1 = 151.2093;
        var lat2 = -37.8136; // Melbourne
        var lon2 = 144.9631;

        // Act
        var distance = Calcul.CalculateHaversineDistance(lat1, lon1, lat2, lon2);

        // Assert
        Assert.InRange(distance, 713, 715); // The distance between Sydney and Melbourne is approximately 713 km
    }
}