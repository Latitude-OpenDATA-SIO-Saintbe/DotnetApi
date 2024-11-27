using Xunit;
using Metheo.Tools;

namespace Metheo.Tests
{
    /// <summary>
    /// Contains unit tests for the <see cref="Calcul"/> class.
    /// </summary>
    public class CalculTest
    {
        /// <summary>
        /// Tests the <see cref="Calcul.CalculateHaversineDistance"/> method with valid coordinates.
        /// Verifies that the calculated distance between Berlin and Paris is within the expected range.
        /// </summary>
        [Fact]
        public void CalculateHaversineDistance_ValidCoordinates_ReturnsCorrectDistance()
        {
            // Arrange
            double lat1 = 52.5200; // Berlin
            double lon1 = 13.4050;
            double lat2 = 48.856; // Paris
            double lon2 = 62.3522;

            // Act
            double result = Calcul.CalculateHaversineDistance(lat1, lon1, lat2, lon2);

            // Assert
            Assert.InRange(result, 3405, 3420); // Distance between Berlin and Paris is approximately 878 km
        }

        /// <summary>
        /// Tests the <see cref="Calcul.CalculateHaversineDistance"/> method with identical coordinates.
        /// Verifies that the calculated distance is zero.
        /// </summary>
        [Fact]
        public void CalculateHaversineDistance_SameCoordinates_ReturnsZero()
        {
            // Arrange
            double lat1 = 52.5200; // Berlin
            double lon1 = 13.4050;
            double lat2 = 52.5200; // Berlin
            double lon2 = 13.4050;

            // Act
            double result = Calcul.CalculateHaversineDistance(lat1, lon1, lat2, lon2);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests the <see cref="Calcul.CalculateHaversineDistance"/> method with negative coordinates.
        /// Verifies that the calculated distance between Sydney and Melbourne is within the expected range.
        /// </summary>
        [Fact]
        public void CalculateHaversineDistance_NegativeCoordinates_ReturnsCorrectDistance()
        {
            // Arrange
            double lat1 = -33.8688; // Sydney
            double lon1 = 151.2093;
            double lat2 = -37.8136; // Melbourne
            double lon2 = 144.9631;

            // Act
            double result = Calcul.CalculateHaversineDistance(lat1, lon1, lat2, lon2);

            // Assert
            Assert.InRange(result, 713, 715); // Distance between Sydney and Melbourne is approximately 714 km
        }

        /// <summary>
        /// Tests the <see cref="Calcul.CalculateHaversineDistance"/> method with zero coordinates.
        /// Verifies that the calculated distance is zero.
        /// </summary>
        [Fact]
        public void CalculateHaversineDistance_ZeroCoordinates_ReturnsCorrectDistance()
        {
            // Arrange
            double lat1 = 0.0; // Equator
            double lon1 = 0.0;
            double lat2 = 0.0; // Equator
            double lon2 = 0.0;

            // Act
            double result = Calcul.CalculateHaversineDistance(lat1, lon1, lat2, lon2);

            // Assert
            Assert.Equal(0, result);
        }
    }
}
