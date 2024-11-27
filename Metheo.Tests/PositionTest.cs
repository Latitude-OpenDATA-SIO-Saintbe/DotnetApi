using System;
using Xunit;
using Metheo.Tools;

/// <summary>
/// Unit tests for the <see cref="Position"/> class.
/// </summary>
namespace Metheo.Tests
{
    public class PositionTest
    {
        /// <summary>
        /// Tests the <see cref="Position.RetrievePosition(double, double)"/> method.
        /// Verifies that given valid latitude and longitude, the method returns the correct coordinates.
        /// </summary>
        [Fact]
        public void RetrievePosition_ValidPosition_ReturnsCoordinates()
        {
            // Arrange
            double lat = 45.0;
            double lon = 90.0;

            // Act
            var result = Position.RetrievePosition(lat, lon);

            // Assert
            Assert.Equal(45.0, result.lat);
            Assert.Equal(90.0, result.lon);
        }
    }
}
