using Metheo.BL;
using Metheo.DAL;
using Metheo.DTO;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Language;
using Moq.Language.Flow;

namespace Metheo.Tests.BL;

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
        var cachedCategories = new List<CategoryType> { new() { Name = "Temperature" } };
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
    public async Task GetMultipleCategoryTypesAsync_ReturnsCategoriesFromCache()
    {
        var cachedCategories = new List<CategoryType>
        {
            new() { Name = "Temperature" },
            new() { Name = "Wind" }
        };
        object cacheEntry = cachedCategories;

        _cacheMock
            .Setup(x => x.TryGetValue("CategoryTypes", out cacheEntry))
            .Returns(true);

        var result = await _weatherService.GetCategoryTypesAsync();

        Assert.Equal(cachedCategories, result);
    }

    [Fact]
    public async Task GetMultipleCategoryTypesAsync_ReturnsCategoriesFromRepository()
    {
        var categories = new List<string> { "Temperature", "Wind" };
        _repositoryMock.Setup(x => x.GetCategoryTypesAsync()).ReturnsAsync(categories);

        var cacheEntry = new List<CategoryType>();
        _cacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        var result = await _weatherService.GetCategoryTypesAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Temperature", result[0].Name);
        Assert.Equal("Wind", result[1].Name);
    }

    [Fact]
    public async Task GetCategoryTypesAsync_ThrowsException_WhenNoCategoriesFound()
    {
        // Arrange
        var repositoryMock = new Mock<IWeatherRepository>();
        var cacheMock = new Mock<IMemoryCache>();

        repositoryMock.Setup(r => r.GetCategoryTypesAsync()).ReturnsAsync((List<string>?)null);

        var service = new WeatherService(repositoryMock.Object, cacheMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.GetCategoryTypesAsync());
    }

    [Fact]
    public async Task PostCitiesAndDepartmentsAsync_ReturnsCombinedList()
    {
        var cities = new List<Cities> { new() { Id = 1, Name = "City1" } };
        var departments = new List<Department> { new() { Id = "1", Name = "Department1" } };
        _repositoryMock.Setup(x => x.GetCitiesAsync(It.IsAny<string>())).ReturnsAsync(cities);
        _repositoryMock.Setup(x => x.GetDepartmentsAsync(It.IsAny<string>())).ReturnsAsync(departments);

        var result = await _weatherService.PostCitiesAndDepartmentsAsync("search");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task PostCitiesAndDepartmentsAsync_ReturnsMultipleCombinedList()
    {
        var cities = new List<Cities>
        {
            new() { Id = 1, Name = "City1" },
            new() { Id = 2, Name = "City2" }
        };
        var departments = new List<Department>
        {
            new() { Id = "1", Name = "Department1" },
            new() { Id = "2", Name = "Department2" },
            new() { Id = "3", Name = "Department3" }
        };
        _repositoryMock.Setup(x => x.GetCitiesAsync(It.IsAny<string>())).ReturnsAsync(cities);
        _repositoryMock.Setup(x => x.GetDepartmentsAsync(It.IsAny<string>())).ReturnsAsync(departments);

        var result = await _weatherService.PostCitiesAndDepartmentsAsync("search");

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetCityOrDepartmentPositionAsync_ReturnsCityPosition()
    {
        var city = new Cities { Latitude = (float)25.0, Longitude = (float)25.0 };
        _repositoryMock.Setup(x => x.GetCityByNameAsync(It.IsAny<string>())).ReturnsAsync(city);

        var result = await _weatherService.GetCityOrDepartmentPositionAsync("city");

        Assert.Equal(25.0, result.Latitude);
        Assert.Equal(25.0, result.Longitude);
    }

    [Fact]
    public async Task GetCityOrDepartmentPositionAsync_ReturnsDepartementPosition()
    {
        var department = new Department { Latitude = (float)25.0, Longitude = (float)25.0 };
        _repositoryMock.Setup(x => x.GetCityByNameAsync(It.IsAny<string>())).ReturnsAsync((Cities)null);
        _repositoryMock.Setup(x => x.GetDepartmentByNameAsync(It.IsAny<string>())).ReturnsAsync(department);

        var result = await _weatherService.GetCityOrDepartmentPositionAsync("city");

        Assert.Equal(25.0, result.Latitude);
        Assert.Equal(25.0, result.Longitude);
    }

    [Fact]
    public async Task GetCityOrDepartmentPositionAsync_ReturnsNotFound()
    {
        _repositoryMock.Setup(x => x.GetCityByNameAsync(It.IsAny<string>())).ReturnsAsync((Cities)null);
        _repositoryMock.Setup(x => x.GetDepartmentByNameAsync(It.IsAny<string>())).ReturnsAsync((Department)null);

        var result = await _weatherService.GetCityOrDepartmentPositionAsync("city");

        Assert.Null(result);
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
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _weatherService.GetWeatherData("2023-01-01:2023-01-31", "invalid", "20", null));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _weatherService.GetWeatherData("2023-01-01:2023-01-31", "10", "invalid", null));
    }

    [Fact]
    public async Task NoWeatherDataFound_ThrowsException()
    {
        // Arrange
        var repositoryMock = new Mock<IWeatherRepository>();
        repositoryMock.Setup(r => r.GetWeatherDataByPositionAsync(It.IsAny<double>(), It.IsAny<double>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<CategorySearch>>()))
            .ReturnsAsync(new List<WeatherDataResponse>());
        var cacheMock = new Mock<IMemoryCache>();
        var weatherService = new WeatherService(repositoryMock.Object, cacheMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            weatherService.GetWeatherData("01-01-2023:02-01-2023", "48.8566", "2.3522", null));
        Assert.Equal("No weather data found for the specified criteria.", exception.Message);
    }

    [Fact]
    public async Task NoWeatherGlobalDataFound_ThrowsException()
    {
        // Arrange
        var repositoryMock = new Mock<IWeatherRepository>();
        repositoryMock.Setup(r => r.GetGlobalWeatherDataAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<CategorySearch>>()))
            .ReturnsAsync(new List<WeatherDataResponse>());
        var cacheMock = new Mock<IMemoryCache>();
        var weatherService = new WeatherService(repositoryMock.Object, cacheMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            weatherService.GetWeatherData("01-01-2023:02-01-2023", "France", "France", null));
        Assert.Equal("No weather data found for the specified criteria.", exception.Message);
    }

    [Fact]
    public async Task NoValidWeatherGlobalDataPosition_ThrowsException()
    {
        // Arrange
        var repositoryMock = new Mock<IWeatherRepository>();
        var cacheMock = new Mock<IMemoryCache>();
        var weatherService = new WeatherService(repositoryMock.Object, cacheMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            weatherService.GetWeatherData("01-01-2023:02-01-2023", "Mae", "Mae", null));
        Assert.Equal($"Invalid latitude or longitude values: (Mae, Mae).", exception.Message);
    }

    [Fact]
    public async Task GetWeatherData_ReturnsGlobalWeatherData_WhenLatitudeAndLongitudeAreFranceWithCache()
    {
        // Arrange
        var repositoryMock = new Mock<IWeatherRepository>();
        var cacheMock = new Mock<IMemoryCache>();
        var weatherService = new WeatherService(repositoryMock.Object, cacheMock.Object);
        var latitude = "france";
        var longitude = "france";
        var dateRange = "01-01-2023:31-01-2023";
        string? category = null;
        var expectedWeatherData = new List<WeatherDataResponse>
        {
            new(),
            new()
        };
        object cacheEntry = expectedWeatherData;

        // Setup cache to return the weather data
        cacheMock.Setup(x => x.TryGetValue("GlobalWeatherData", out cacheEntry)).Returns(true);

        // Act
        var result = await weatherService.GetWeatherData(dateRange, latitude, longitude, category);

        // Assert
        Assert.Equal(expectedWeatherData, result);
    }

    [Fact]
    public async Task GetWeatherData_ReturnsGlobalWeatherData_WhenLatitudeAndLongitudeAreFranceFromRepository()
    {
        // Arrange
        var repositoryMock = new Mock<IWeatherRepository>();
        var cacheMock = new Mock<IMemoryCache>();
        var weatherService = new WeatherService(repositoryMock.Object, cacheMock.Object);
        var latitude = "france";
        var longitude = "france";
        var dateRange = "01-01-2023:31-01-2023";
        string? category = null;
        var expectedWeatherData = new List<WeatherDataResponse>
        {
            new(),
            new()
        };

        // Setup repository to return the weather data
        repositoryMock.Setup(r =>
                r.GetGlobalWeatherDataAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                    It.IsAny<List<CategorySearch>>()))
            .ReturnsAsync(expectedWeatherData);

        object cacheEntry = null;
        cacheMock.Setup(x => x.TryGetValue("GlobalWeatherData", out cacheEntry)).Returns(false);
        cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());

        // Act
        var result = await weatherService.GetWeatherData(dateRange, latitude, longitude, category);

        // Assert
        Assert.Equal(expectedWeatherData, result);
    }

    [Fact]
    public async Task GetGlobalWeatherData_WithCategory()
    {
        // Arrange
        var repositoryMock = new Mock<IWeatherRepository>();
        var cacheMock = new Mock<IMemoryCache>();
        var weatherService = new WeatherService(repositoryMock.Object, cacheMock.Object);

        var latitude = "france";
        var longitude = "france";
        var dateRange = "01-01-2023:31-01-2023";
        string category = "temperature_2m&wind_gusts_10m";  // Category that needs to be passed

        // Mock the repository's response when fetching global weather data
        var expectedWeatherData = new List<WeatherDataResponse>
        {
            new WeatherDataResponse { Id = 1,  temperature_2m = [25f], wind_gusts_10m = [10f] },
        };

        // Setup the repository mock to return global weather data for this date range and categories
        repositoryMock.Setup(r => r.GetGlobalWeatherDataAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<List<CategorySearch>>()))
            .ReturnsAsync(expectedWeatherData);

        // Mock GetCategoryTypesAsync
        var categoryTypes = new List<CategoryType> { new() { Name = "temperature_2m" }, new() { Name = "wind_gusts_10m" } };
        repositoryMock.Setup(x => x.GetCategoryTypesAsync()).ReturnsAsync(new List<string> { "temperature_2m", "wind_gusts_10m" });

        // Mock the cache entry creation
        var cacheEntryMock = new Mock<ICacheEntry>();
        cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

        // Act
        var result = await weatherService.GetWeatherData(dateRange, latitude, longitude, category);

        // Assert
        Assert.Equal(expectedWeatherData.Count, result.Count());
        Assert.Contains(result, data => data.temperature_2m[0] == 25f);
        Assert.Contains(result, data => data.wind_gusts_10m[0] == 10f);
    }

    [Fact]
    public async Task GetGlobalWeatherMultipleData_WithCategory()
    {
        // Arrange
        var repositoryMock = new Mock<IWeatherRepository>();
        var cacheMock = new Mock<IMemoryCache>();
        var weatherService = new WeatherService(repositoryMock.Object, cacheMock.Object);

        var latitude = "france";
        var longitude = "france";
        var dateRange = "01-01-2023:31-01-2023";
        string category = "temperature_2m&wind_gusts_10m";  // Category that needs to be passed

        // Mock the repository's response when fetching global weather data
        var expectedWeatherData = new List<WeatherDataResponse>
        {
            new WeatherDataResponse { Id = 1,  temperature_2m = [25f, 30f], wind_gusts_10m = [10f, 15f] },
        };

        // Setup the repository mock to return global weather data for this date range and categories
        repositoryMock.Setup(r => r.GetGlobalWeatherDataAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<List<CategorySearch>>()))
            .ReturnsAsync(expectedWeatherData);

        // Mock GetCategoryTypesAsync
        var categoryTypes = new List<CategoryType> { new() { Name = "temperature_2m" }, new() { Name = "wind_gusts_10m" } };
        repositoryMock.Setup(x => x.GetCategoryTypesAsync()).ReturnsAsync(new List<string> { "temperature_2m", "wind_gusts_10m" });

        // Mock the cache entry creation
        var cacheEntryMock = new Mock<ICacheEntry>();
        cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

        // Act
        var result = await weatherService.GetWeatherData(dateRange, latitude, longitude, category);

        // Assert
        Assert.Equal(expectedWeatherData.Count, result.Count());
        Assert.Contains(result, data => data.temperature_2m[0] == 25f);
        Assert.Contains(result, data => data.wind_gusts_10m[0] == 10f);
        Assert.Contains(result, data => data.temperature_2m[1] == 30f);
        Assert.Contains(result, data => data.wind_gusts_10m[1] == 15f);
    }

    [Fact]
    public async Task GetWeatherData_WithCategory()
    {
        // Arrange
        var repositoryMock = new Mock<IWeatherRepository>();
        var cacheMock = new Mock<IMemoryCache>();
        var weatherService = new WeatherService(repositoryMock.Object, cacheMock.Object);

        var latitude = "48.560";
        var longitude = "2.3522";
        var dateRange = "01-01-2023:31-01-2023";
        string category = "temperature_2m&wind_gusts_10m";  // Category that needs to be passed

        // Mock the repository's response when fetching global weather data
        var expectedWeatherData = new List<WeatherDataResponse>
        {
            new WeatherDataResponse { Id = 1,  temperature_2m = [25f], wind_gusts_10m = [10f] },
        };

        // Setup the repository mock to return global weather data for this date range and categories
        repositoryMock.Setup(r => r.GetWeatherDataByPositionAsync(
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<List<CategorySearch>>()))
            .ReturnsAsync(expectedWeatherData);

        // Mock GetCategoryTypesAsync
        var categoryTypes = new List<CategoryType> { new() { Name = "temperature_2m" }, new() { Name = "wind_gusts_10m" } };
        repositoryMock.Setup(x => x.GetCategoryTypesAsync()).ReturnsAsync(new List<string> { "temperature_2m", "wind_gusts_10m" });

        // Mock the cache entry creation
        var cacheEntryMock = new Mock<ICacheEntry>();
        cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

        // Act
        var result = await weatherService.GetWeatherData(dateRange, latitude, longitude, category);

        // Assert
        Assert.Equal(expectedWeatherData.Count, result.Count());
        Assert.Contains(result, data => data.temperature_2m[0] == 25f);
        Assert.Contains(result, data => data.wind_gusts_10m[0] == 10f);
    }

        [Fact]
    public async Task GetWeatherMultipleData_WithCategory()
    {
        // Arrange
        var repositoryMock = new Mock<IWeatherRepository>();
        var cacheMock = new Mock<IMemoryCache>();
        var weatherService = new WeatherService(repositoryMock.Object, cacheMock.Object);

        var latitude = "48.560";
        var longitude = "2.3522";
        var dateRange = "01-01-2023:31-01-2023";
        string category = "temperature_2m&wind_gusts_10m";  // Category that needs to be passed

        // Mock the repository's response when fetching global weather data
        var expectedWeatherData = new List<WeatherDataResponse>
        {
            new WeatherDataResponse { Id = 1,  temperature_2m = [25f,30f], wind_gusts_10m = [10f,15f] },
        };

        // Setup the repository mock to return global weather data for this date range and categories
        repositoryMock.Setup(r => r.GetWeatherDataByPositionAsync(
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<List<CategorySearch>>()))
            .ReturnsAsync(expectedWeatherData);

        // Mock GetCategoryTypesAsync
        var categoryTypes = new List<CategoryType> { new() { Name = "temperature_2m" }, new() { Name = "wind_gusts_10m" } };
        repositoryMock.Setup(x => x.GetCategoryTypesAsync()).ReturnsAsync(new List<string> { "temperature_2m", "wind_gusts_10m" });

        // Mock the cache entry creation
        var cacheEntryMock = new Mock<ICacheEntry>();
        cacheMock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

        // Act
        var result = await weatherService.GetWeatherData(dateRange, latitude, longitude, category);

        // Assert
        Assert.Equal(expectedWeatherData.Count, result.Count());
        Assert.Contains(result, data => data.temperature_2m[0] == 25f);
        Assert.Contains(result, data => data.wind_gusts_10m[0] == 10f);
        Assert.Contains(result, data => data.temperature_2m[1] == 30f);
        Assert.Contains(result, data => data.wind_gusts_10m[1] == 15f);
    }

    [Fact]
    public async Task GetWeatherData_ThrowsException_WhenNoWeatherDataFound()
    {
        // Arrange
        var repositoryMock = new Mock<IWeatherRepository>();
        var cacheMock = new Mock<IMemoryCache>();
        var weatherService = new WeatherService(repositoryMock.Object, cacheMock.Object);
        var latitude = "48.8566";
        var longitude = "2.3522";
        var dateRange = "2023-01-01:2023-01-31";
        string? category = null;
        repositoryMock.Setup(r => r.GetWeatherDataByPositionAsync(It.IsAny<double>(), It.IsAny<double>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<CategorySearch>>()))
            .ReturnsAsync(new List<WeatherDataResponse>());

        // Act & Assert
        await Assert.ThrowsAsync<System.ArgumentException>(() =>
            weatherService.GetWeatherData(dateRange, latitude, longitude, category));
    }

    [Fact]
    public async Task GetWeatherData_ReturnsWeatherDataWithCache()
    {
        var weatherData = new List<WeatherDataResponse> { new() { Id = 1 } };
        object cacheEntry = weatherData;

        // Setup cache to return the weather data
        _cacheMock.Setup(x => x.TryGetValue("WeatherData", out cacheEntry)).Returns(true);

        var result = await _weatherService.GetWeatherData("01-01-2023:31-01-2023", "10", "20", null);

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public async Task GetWeatherData_ReturnsWeatherDataFromRepository()
    {
        var weatherData = new List<WeatherDataResponse> { new() { Id = 1 } };

        var cacheEntry = new List<CategoryType>();
        _cacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        _repositoryMock
            .Setup(x => x.GetWeatherDataByPositionAsync(It.IsAny<double>(), It.IsAny<double>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<List<CategorySearch>>())).ReturnsAsync(weatherData);

        var result = await _weatherService.GetWeatherData("01-01-2023:31-01-2023", "10", "20", null);

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public async Task GetWeatherData_ReturnsMultipleWeatherDataWithCache()
    {
        var weatherData = new List<WeatherDataResponse>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };
        object cacheEntry = weatherData;

        // Setup cache to return the weather data
        _cacheMock.Setup(x => x.TryGetValue("WeatherData", out cacheEntry)).Returns(true);

        var result = await _weatherService.GetWeatherData("01-01-2023:31-01-2023", "10", "20", null);

        Assert.Equal(2, result.Count());
        Assert.Equal(1, result.First().Id);
        Assert.Equal(2, result.Last().Id);
    }


    [Fact]
    public async Task GetCategoryName_ThrowsException_WhenCategoryNameNotInDatabase()
    {
        // Arrange
        var repositoryMock = new Mock<IWeatherRepository>();
        var cacheMock = new Mock<IMemoryCache>();
        var weatherService = new WeatherService(repositoryMock.Object, cacheMock.Object);
        var categoryName = "pluie&vent";
        var categoryTypes = new List<CategoryType> { new() { Name = "pluie" }, new() { Name = "vent" } };
        _repositoryMock.Setup(x => x.GetCategoryTypesAsync()).ReturnsAsync(new List<string> { "pluie", "vent" });

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => weatherService.GetCategoryName(categoryName));
    }

    [Fact]
    public async Task GetCategoryName_ThrowsException_WhenCategoryNameIsEmpty()
    {
        // Arrange
        var repositoryMock = new Mock<IWeatherRepository>();
        var cacheMock = new Mock<IMemoryCache>();
        var weatherService = new WeatherService(repositoryMock.Object, cacheMock.Object);
        var categoryName = "";

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => weatherService.GetCategoryName(categoryName));
    }

    [Fact]
    public async Task GetWeatherData_ReturnsGlobalWeatherData_WithCache()
    {
        var weatherData = new List<WeatherDataResponse> { new() { Id = 1 } };
        object cacheEntry = weatherData;

        // Setup cache to return the weather data
        _cacheMock.Setup(x => x.TryGetValue("GlobalWeatherData", out cacheEntry)).Returns(true);

        var result = await _weatherService.GetWeatherData("01-01-2023:31-01-2023", "France", "France", null);

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public async Task GetWeatherData_ReturnsGlobalWeatherData_FromRepository()
    {
        var weatherData = new List<WeatherDataResponse> { new() { Id = 1 } };

        // Setup cache to return false
        var cacheEntry = new List<CategoryType>();
        _cacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        // Setup repository to return the weather data
        _repositoryMock
            .Setup(x => x.GetGlobalWeatherDataAsync(It.IsAny<DateTime>(), It.IsAny<DateTime?>(),
                It.IsAny<List<CategorySearch>>())).ReturnsAsync(weatherData);

        var result = await _weatherService.GetWeatherData("01-01-2023:31-01-2023", "France", "France", null);

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public async Task GetCategoryName_ReturnsCategoryList_WhenCategoriesExist()
    {
        // Arrange
        var categoryTypes = new List<CategoryType> { new() { Name = "pluie" }, new() { Name = "vent" } };
        _repositoryMock.Setup(x => x.GetCategoryTypesAsync()).ReturnsAsync(new List<string> { "pluie", "vent" });
        object cacheEntry = categoryTypes;
        _cacheMock.Setup(x => x.TryGetValue("CategoryTypes", out cacheEntry!)).Returns(true);

        // Act
        var result = await _weatherService.GetCategoryName("pluie&vent");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "pluie");
        Assert.Contains(result, c => c.Name == "vent");
    }

    [Fact]
    public async Task GetCategoryName_ThrowsException_WhenCategoryNotFound()
    {
        // Arrange
        var categoryTypes = new List<CategoryType> { new() { Name = "pluie" } };
        _repositoryMock.Setup(x => x.GetCategoryTypesAsync()).ReturnsAsync(new List<string> { "pluie" });
        object cacheEntry = categoryTypes;
        _cacheMock.Setup(x => x.TryGetValue("CategoryTypes", out cacheEntry!)).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _weatherService.GetCategoryName("pluie&vent"));
        Assert.Equal("Category name 'vent' is not in the database.", exception.Message);
    }
}