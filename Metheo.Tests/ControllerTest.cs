using System.Net;
using Microsoft.AspNetCore.Mvc;
using Metheo.Api.Controllers;
using Metheo.Api.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;

namespace Metheo.Tests
{
    /// <summary>
    /// Unit tests for the WeatherController class.
    /// </summary>
    public class WeatherControllerTests
    {
        private readonly Mock<WeatherDbContext> _mockContext;
        private readonly WeatherController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherControllerTests"/> class.
        /// </summary>
        public WeatherControllerTests()
        {
            _mockContext = new Mock<WeatherDbContext>();
            _controller = new WeatherController(_mockContext.Object);
        }

        /// <summary>
        /// Tests that GetCategoryTypes returns an OkResult with a list of category types.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task GetCategoryTypes_ReturnsOkResult_WithListOfCategoryTypes()
        {
            // Arrange
            var mockConnection = new Mock<NpgsqlConnection>();
            var mockCommand = new Mock<NpgsqlCommand>();
            var mockReader = new Mock<NpgsqlDataReader>();

            mockReader.SetupSequence(r => r.ReadAsync())
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockReader.Setup(r => r.GetString(0)).Returns("Temperature");

            mockCommand.Setup(c => c.ExecuteReaderAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockReader.Object);
            mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

            _mockContext.Setup(c => c.Database.GetConnectionString()).Returns("Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase");

            // Act
            var result = await _controller.GetCategoryTypes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var categoryTypes = Assert.IsType<List<string>>(okResult.Value);
            Assert.Single(categoryTypes);
            Assert.Equal("Temperature", categoryTypes[0]);
        }

        /// <summary>
        /// Tests that GetCities returns an OkResult with a list of cities and departments.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task GetCities_ReturnsOkResult_WithListOfCitiesAndDepartments()
        {
            // Arrange
            var cities = new List<City> { new City { Name = "City1" } };
            var departments = new List<Departement> { new Departement { Name = "Department1" } };

            _mockContext.Setup(c => c.Cities.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(cities);
            _mockContext.Setup(c => c.Departements.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(departments);

            // Act
            var result = await _controller.GetCities();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var combinedList = Assert.IsType<List<object>>(okResult.Value);
            Assert.Equal(2, combinedList.Count);
        }

        /// <summary>
        /// Tests that GetWeatherData returns a BadRequest when the date range is empty.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task GetWeatherData_ReturnsBadRequest_WhenDateRangeIsEmpty()
        {
            // Act
            var result = await _controller.GetWeatherData("");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Date range cannot be empty.", badRequestResult.Value);
        }

        /// <summary>
        /// Tests that GetWeatherData returns a BadRequest when the date range is invalid.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task GetWeatherData_ReturnsBadRequest_WhenDateRangeIsInvalid()
        {
            // Act
            var result = await _controller.GetWeatherData("invalid-date-range");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid date range format. Use 'YYYY-MM-DD to YYYY-MM-DD'.", badRequestResult.Value);
        }

        /// <summary>
        /// Tests that GetWeatherData returns a NotFound when no weather data is available for France.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task GetWeatherData_ReturnsNotFound_WhenNoWeatherDataForFrance()
        {
            // Arrange
            var dateRange = "2023-01-01:2023-01-31";
            var city = "France";
            var category = "Temperature";

            _mockContext.Setup(c => c.Departements.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Departement>());

            // Act
            var result = await _controller.GetWeatherData(dateRange, city, category);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("No weather data available for France.", notFoundResult.Value);
        }

        /// <summary>
        /// Tests that GetWeatherData returns an InternalServerError when there is an error fetching weather data.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task GetWeatherData_ReturnsInternalServerError_WhenErrorFetchingWeatherData()
        {
            // Arrange
            var dateRange = "2023-01-01:2023-01-31";
            var city = "City1";
            var category = "Temperature";

            _mockContext.Setup(c => c.WeatherDatas.AsQueryable()).Throws(new Exception("Database error"));

            // Act
            var result = await _controller.GetWeatherData(dateRange, city, category);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("Error fetching weather data", statusCodeResult.Value);
        }
    }
}
