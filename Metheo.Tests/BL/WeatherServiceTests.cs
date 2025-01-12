namespace Metheo.Tests.BL;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metheo.BL;
using Metheo.DAL;
using DTO;
using Microsoft.AspNetCore.Mvc;  
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

public class WeatherServiceTests
{
    private readonly Mock<IWeatherRepository> _repositoryMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly WeatherService _weatherService;

    public WeatherServiceTests()
    {
        _repositoryMock = new Mock<IWeatherRepository>();
        _cacheMock = new Mock<IMemoryCache>();
        _weatherService = new WeatherService(_repositoryMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task GetCategoryTypesAsync_ReturnsCategoriesFromCache()
    {
        var cachedCategories = new List<CategoryType> { new CategoryType { Name = "Temperature" } };
        object cacheEntry = cachedCategories;

        _cacheMock
            .Setup(x => x.TryGetValue("CategoryTypes", out cacheEntry))
            .Returns(true);

        var result = await _weatherService.GetCategoryTypesAsync();

        Assert.Equal(cachedCategories, result);
    }

    [Fact]
    public async Task GetCategoryTypesAsync_ReturnsCategoriesFromRepository()
    {
        var categories = new List<string> { "Temperature" };
        _repositoryMock.Setup(x => x.GetCategoryTypesAsync()).ReturnsAsync(categories);

        var cacheEntry = new List<CategoryType>();
        _cacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        var result = await _weatherService.GetCategoryTypesAsync();

        Assert.Single(result);
        Assert.Equal("Temperature", result[0].Name);
    }

    [Fact]
    public async Task PostCitiesAndDepartmentsAsync_ReturnsCombinedList()
    {
        var cities = new List<Cities> { new Cities { Id = 1, Name = "City1" } };
        var departments = new List<Department> { new Department { Id = "1", Name = "Department1" } };
        _repositoryMock.Setup(x => x.GetCitiesAsync(It.IsAny<string>())).ReturnsAsync(cities);
        _repositoryMock.Setup(x => x.GetDepartmentsAsync(It.IsAny<string>())).ReturnsAsync(departments);

        var result = await _weatherService.PostCitiesAndDepartmentsAsync("search");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetCityOrDepartmentPositionAsync_ReturnsCityPosition()
    {
        var city = new Cities { Latitude = 10, Longitude = 20 };
        _repositoryMock.Setup(x => x.GetCityByNameAsync(It.IsAny<string>())).ReturnsAsync(city);

        var result = await _weatherService.GetCityOrDepartmentPositionAsync("city");

        var okResult = Assert.IsType<LocationDto>(result.Value);
        Assert.Equal(10, okResult.Latitude);
        Assert.Equal(20, okResult.Longitude);
    }

    [Fact]
    public async Task GetCityOrDepartmentPositionAsync_ReturnsNotFound()
    {
        _repositoryMock.Setup(x => x.GetCityByNameAsync(It.IsAny<string>())).ReturnsAsync((Cities)null);
        _repositoryMock.Setup(x => x.GetDepartmentByNameAsync(It.IsAny<string>())).ReturnsAsync((Department)null);

        var result = await _weatherService.GetCityOrDepartmentPositionAsync("city");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetWeatherData_ThrowsArgumentException_WhenDateRangeIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _weatherService.GetWeatherData("", "10", "20", null));
    }

    [Fact]
    public async Task GetWeatherData_ThrowsArgumentException_WhenDateRangeIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _weatherService.GetWeatherData("invalid", "10", "20", null));
    }

    [Fact]
    public async Task GetWeatherData_ThrowsArgumentException_WhenLatitudeOrLongitudeIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _weatherService.GetWeatherData("2023-01-01:2023-01-31", "invalid", "20", null));
        await Assert.ThrowsAsync<ArgumentException>(() => _weatherService.GetWeatherData("2023-01-01:2023-01-31", "10", "invalid", null));
    }

    [Fact]
    public async Task GetWeatherData_ReturnsWeatherData()
    {
        var weatherData = new List<WeatherDataResponse> { new WeatherDataResponse { Id = 1 } };
        _repositoryMock.Setup(x => x.GetWeatherDataByPositionAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime>(), It.IsAny<DateTime?>(), It.IsAny<List<CategorySearch>>())).ReturnsAsync(weatherData);

        var result = await _weatherService.GetWeatherData("01-01-2023:31-01-2023", "10", "20", null);

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }
}