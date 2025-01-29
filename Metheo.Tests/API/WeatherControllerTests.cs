using System.IO.Compression;
using System.Net;
using System.Text.Json;
using ApiDotnetMetheoOrm.Controller;
using Metheo.BL;
using Metheo.DTO;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Metheo.Tests.API;

public class WeatherControllerTests
{
    private readonly WeatherController _weatherController;
    private readonly Mock<IWeatherService> _weatherServiceMock;

    public WeatherControllerTests()
    {
        _weatherServiceMock = new Mock<IWeatherService>();
        _weatherController = new WeatherController(_weatherServiceMock.Object);
    }

    [Fact]
    public async Task GetCategoryTypes_ReturnsOkResultWithCategoryTypes()
    {
        // Arrange
        var categoryTypes = new List<CategoryType> { new() { Name = "Type1" }, new() { Name = "Type2" } };
        _weatherServiceMock.Setup(x => x.GetCategoryTypesAsync()).ReturnsAsync(categoryTypes);

        // Act
        var result = await _weatherController.GetCategoryTypes();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
        Assert.Equal(categoryTypes.Select(c => c.Name), returnValue);
    }

    [Fact]
    public async Task PostCities_ReturnsOkResultWithCitiesAndDepartments()
    {
        // Arrange
        var citiesAndDepartments = new List<object>
            { new { City = "City1", Department = "Department1" }, new { City = "City2", Department = "Department2" } };
        _weatherServiceMock.Setup(x => x.PostCitiesAndDepartmentsAsync("searchCity"))
            .ReturnsAsync(citiesAndDepartments);

        // Act
        var result = await _weatherController.PostCities("searchCity");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<object>>(okResult.Value);
        Assert.Equal(citiesAndDepartments, returnValue);
    }

    [Fact]
    public async Task GetCityPosition_ReturnsOkResultWithPosition()
    {
        // Arrange
        var position = new LocationDto { Latitude = 25.0, Longitude = 25.0 };
        _weatherServiceMock.Setup(x => x.GetCityOrDepartmentPositionAsync("city")).ReturnsAsync(position);

        // Act
        var result = await _weatherController.GetCityPosition("city");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<LocationDto>(okResult.Value);
        Assert.Equal(position, returnValue);
    }

    [Fact]
    public async Task GetCityPosition_ReturnsNotFound_WhenPositionNotFound()
    {
        // Arrange
        _weatherServiceMock.Setup(x => x.GetCityOrDepartmentPositionAsync("city"))
            .ReturnsAsync((LocationDto)null!); // Mocking the service to return null

        // Act
        var result = await _weatherController.GetCityPosition("city");

        // Assert
        Assert.IsType<NotFoundResult>(result
            .Result); // Ensure that a NotFoundResult is returned when position is not found
    }

    [Fact]
    public async Task GetWeatherData_ReturnsFileContentResultWithWeatherData()
    {
        // Arrange
        var weatherData = new List<WeatherDataResponse>
        {
            new() { time = new List<DateTime?> { Convert.ToDateTime("2023-01-01") }, temperature_2m = new List<float?> { 25.0f } }
        };
        _weatherServiceMock.Setup(x => x.GetWeatherData("dateRange", "latitude", "longitude", "category"))
            .ReturnsAsync(weatherData);

        // Act
        var result = await _weatherController.GetWeatherData("dateRange", "latitude", "longitude", "category");

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/gzip", fileResult.ContentType);

        // Decompress and verify the content
        using var memoryStream = new MemoryStream(fileResult.FileContents);
        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
        var decompressedData = await JsonSerializer.DeserializeAsync<List<WeatherDataResponse>>(gzipStream);

        Assert.Collection(decompressedData,
            item =>
            {
                Assert.Equal(weatherData[0].time, item.time);
                Assert.Equal(weatherData[0].temperature_2m, item.temperature_2m);
            });
    }

    [Fact]
    public async Task GetWeatherData_ReturnsFileContentResultWithMultipleWeatherData()
    {
        // Arrange
        var weatherData = new List<WeatherDataResponse>
        {
            new() { time = new List<DateTime?> { Convert.ToDateTime("2023-01-01") }, temperature_2m = new List<float?> { 25.0f } },
            new() { time = new List<DateTime?> { Convert.ToDateTime("2023-01-02") }, temperature_2m = new List<float?> { 26.0f } }
        };
        _weatherServiceMock.Setup(x => x.GetWeatherData("dateRange", "latitude", "longitude", "category"))
            .ReturnsAsync(weatherData);

        // Act
        var result = await _weatherController.GetWeatherData("dateRange", "latitude", "longitude", "category");

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/gzip", fileResult.ContentType);

        // Decompress and verify the content
        using var memoryStream = new MemoryStream(fileResult.FileContents);
        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
        var decompressedData = await JsonSerializer.DeserializeAsync<List<WeatherDataResponse>>(gzipStream);

        Assert.Collection(decompressedData,
            item =>
            {
                Assert.Equal(weatherData[0].time, item.time);
                Assert.Equal(weatherData[0].temperature_2m, item.temperature_2m);
            },
            item =>
            {
                Assert.Equal(weatherData[1].time, item.time);
                Assert.Equal(weatherData[1].temperature_2m, item.temperature_2m);
            });
    }

    [Fact]
    public async Task GetWeatherData_InvalidArgument_ReturnsBadRequest()
    {
        // Arrange
        _weatherServiceMock.Setup(x => x.GetWeatherData("dateRange", "latitude", "longitude", "category"))
            .ThrowsAsync(new ArgumentException("Invalid argument"));

        // Act
        var result = await _weatherController.GetWeatherData("dateRange", "latitude", "longitude", "category");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid argument", badRequestResult.Value);
    }

    [Fact]
    public async Task GetWeatherData_InternalServerError_ReturnsInternalServerError()
    {
        // Arrange
        _weatherServiceMock.Setup(x => x.GetWeatherData("dateRange", "latitude", "longitude", "category"))
            .ThrowsAsync(new Exception());

        // Act
        var result = await _weatherController.GetWeatherData("dateRange", "latitude", "longitude", "category");

        // Assert
        var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, internalServerErrorResult.StatusCode);
        Assert.Equal("An error occurred while retrieving weather data.", internalServerErrorResult.Value);
    }
}