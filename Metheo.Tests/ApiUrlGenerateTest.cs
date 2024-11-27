using System.Linq.Expressions;
using Metheo.Tools;

namespace Metheo.Tests
{
    /// <summary>
    /// Contains unit tests for the API URL generation functionality in the Metheo.Tools.Api class.
    /// </summary>
    public class ApiUrlGenerateTest
    {
        /// <summary>
        /// Tests that the GenerateApiUrl method returns the correct URL when provided with valid inputs.
        /// </summary>
        [Fact]
        public void GenerateApiUrl_WithValidInputs_ReturnsCorrectUrl()
        {
            // Arrange
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 7);
            var latitude = 40.7128f;
            var longitude = -74.0060f;
            var expectedUrl = "https://api.open-meteo.com/v1/forecast?start_date=2023-01-01&end_date=2023-01-07&latitude=40.7128&longitude=-74.0060";

            // Act
            var result = Metheo.Tools.Api.GenerateApiUrl(startDate, endDate, latitude, longitude);

            // Assert
            Assert.Equal(expectedUrl, result);
        }

        /// <summary>
        /// Tests that the GenerateApiUrl method returns the correct URL when provided with a category.
        /// </summary>
        [Fact]
        public void GenerateApiUrl_WithCategory_ReturnsCorrectUrl()
        {
            // Arrange
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 7);
            var latitude = 40.7128f;
            var longitude = -74.0060f;
            var category = "temperature";
            var expectedUrl = "https://api.open-meteo.com/v1/forecast?start_date=2023-01-01&end_date=2023-01-07&latitude=40.7128&longitude=-74.0060&temperature";

            // Act
            var result = Metheo.Tools.Api.GenerateApiUrl(startDate, endDate, latitude, longitude, category);

            // Assert
            Assert.Equal(expectedUrl, result);
        }

        /// <summary>
        /// Tests that the GenerateApiUrl method returns the correct URL when provided with an empty category.
        /// </summary>
        [Fact]
        public void GenerateApiUrl_WithEmptyCategory_ReturnsCorrectUrl()
        {
            // Arrange
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 7);
            var latitude = 40.7128f;
            var longitude = -74.0060f;
            var category = "";
            var expectedUrl = "https://api.open-meteo.com/v1/forecast?start_date=2023-01-01&end_date=2023-01-07&latitude=40.7128&longitude=-74.0060";

            // Act
            var result = Metheo.Tools.Api.GenerateApiUrl(startDate, endDate, latitude, longitude, category);

            // Assert
            Assert.Equal(expectedUrl, result);
        }

        /// <summary>
        /// Tests that the GenerateApiUrl method returns the correct URL when provided with a null category.
        /// </summary>
        [Fact]
        public void GenerateApiUrl_WithNullCategory_ReturnsCorrectUrl()
        {
            // Arrange
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 7);
            var latitude = 40.7128f;
            var longitude = -74.0060f;
            string? category = null;
            var expectedUrl = "https://api.open-meteo.com/v1/forecast?start_date=2023-01-01&end_date=2023-01-07&latitude=40.7128&longitude=-74.0060";

            // Act
            var result = Metheo.Tools.Api.GenerateApiUrl(startDate, endDate, latitude, longitude, category);

            // Assert
            Assert.Equal(expectedUrl, result);
        }
    }
}
