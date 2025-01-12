namespace Metheo.Tests.Tools;

using Xunit;
using Metheo.Tools;

public class CalculTests
{
    [Fact]
    public void CalculateHaversineDistance_ReturnsCorrectDistance()
    {
        // Arrange
        double lat1 = 52.5200; // Berlin
        double lon1 = 13.4050;
        double lat2 = 48.8566; // Paris
        double lon2 = 2.3522;

        // Act
        double distance = Calcul.CalculateHaversineDistance(lat1, lon1, lat2, lon2);

        // Assert
        Assert.InRange(distance, 877, 880); // The distance between Berlin and Paris is approximately 878 km
    }

    [Fact]
    public void CalculateHaversineDistance_SameLocation_ReturnsZero()
    {
        // Arrange
        double lat = 52.5200; // Berlin
        double lon = 13.4050;

        // Act
        double distance = Calcul.CalculateHaversineDistance(lat, lon, lat, lon);

        // Assert
        Assert.Equal(0, distance);
    }

    [Fact]
    public void CalculateHaversineDistance_NegativeCoordinates_ReturnsCorrectDistance()
    {
        // Arrange
        double lat1 = -33.8688; // Sydney
        double lon1 = 151.2093;
        double lat2 = -37.8136; // Melbourne
        double lon2 = 144.9631;

        // Act
        double distance = Calcul.CalculateHaversineDistance(lat1, lon1, lat2, lon2);

        // Assert
        Assert.InRange(distance, 713, 715); // The distance between Sydney and Melbourne is approximately 713 km
    }
}