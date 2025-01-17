using System.Data;
using Autofac;
using Metheo.DAL;
using Metheo.DTO;
using Moq;

namespace Metheo.Tests.DAL;

public class WeatherRepositoryTests
{
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly Mock<IDapperWrapper> _mockDapperWrapper;
    private readonly WeatherRepository _weatherRepository;

    public WeatherRepositoryTests()
    {
        _mockConnection = new Mock<IDbConnection>();
        _mockDapperWrapper = new Mock<IDapperWrapper>();
        _weatherRepository = new WeatherRepository(new Mock<IComponentContext>().Object, _mockDapperWrapper.Object);
    }

    [Fact]
    public async Task GetCategoryTypesAsync_ReturnsEmptyList_WhenNoCategories()
    {
        var expectedCategories = new List<string>();
        _mockDapperWrapper.Setup(d => d.QueryAsync<string>(_mockConnection.Object, It.IsAny<string>(), null))
            .ReturnsAsync(expectedCategories);

        var result = await _weatherRepository.GetCategoryTypesAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCategoryTypesAsync_ThrowsException_WhenQueryFails()
    {
        _mockDapperWrapper.Setup(d => d.QueryAsync<string>(_mockConnection.Object, It.IsAny<string>(), null))
            .ThrowsAsync(new Exception("Database query failed"));

        await Assert.ThrowsAsync<Exception>(() => _weatherRepository.GetCategoryTypesAsync());
    }

    [Fact]
    public async Task GetCitiesAsync_ReturnsCities()
    {
        var searchCity = "Paris";
        var expectedCities = new List<Cities>
        {
            new() { Id = 1, Name = "Paris", Latitude = 48.8566f, Longitude = 2.3522f }
        };
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<Cities>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(expectedCities);

        var result = await _weatherRepository.GetCitiesAsync(searchCity);

        Assert.Equal(expectedCities, result);
    }

    [Fact]
    public async Task GetCitiesAsync_ReturnsEmptyList_WhenNoCities()
    {
        var searchCity = "NonExistentCity";
        var expectedCities = new List<Cities>();
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<Cities>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(expectedCities);

        var result = await _weatherRepository.GetCitiesAsync(searchCity);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCitiesAsync_ThrowsException_WhenQueryFails()
    {
        var searchCity = "Paris";
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<Cities>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new Exception("Database query failed"));

        await Assert.ThrowsAsync<Exception>(() => _weatherRepository.GetCitiesAsync(searchCity));
    }

    [Fact]
    public async Task GetDepartmentsAsync_ReturnsDepartments()
    {
        var searchCity = "Paris";
        var expectedDepartments = new List<Department>
        {
            new() { Id = "75", Name = "Paris", Latitude = 48.8566f, Longitude = 2.3522f }
        };
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<Department>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(expectedDepartments);

        var result = await _weatherRepository.GetDepartmentsAsync(searchCity);

        Assert.Equal(expectedDepartments, result);
    }

    [Fact]
    public async Task GetDepartmentsAsync_ReturnsEmptyList_WhenNoDepartments()
    {
        var searchCity = "NonExistentCity";
        var expectedDepartments = new List<Department>();
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<Department>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(expectedDepartments);

        var result = await _weatherRepository.GetDepartmentsAsync(searchCity);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDepartmentsAsync_ThrowsException_WhenQueryFails()
    {
        var searchCity = "Paris";
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<Department>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new Exception("Database query failed"));

        await Assert.ThrowsAsync<Exception>(() => _weatherRepository.GetDepartmentsAsync(searchCity));
    }

    [Fact]
    public async Task GetCityByNameAsync_ReturnsCity()
    {
        var cityName = "Paris";
        var expectedCity = new Cities { Id = 1, Name = "Paris", Latitude = 48.8566f, Longitude = 2.3522f };
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<Cities>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(new List<Cities> { expectedCity });

        var result = await _weatherRepository.GetCityByNameAsync(cityName);

        Assert.Equal(expectedCity, result);
    }

    [Fact]
    public async Task GetCityByNameAsync_ReturnsNull_WhenCityNotFound()
    {
        var cityName = "NonExistentCity";
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<Cities>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(new List<Cities>());

        var result = await _weatherRepository.GetCityByNameAsync(cityName);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCityByNameAsync_ThrowsException_WhenQueryFails()
    {
        var cityName = "Paris";
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<Cities>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new Exception("Database query failed"));

        await Assert.ThrowsAsync<Exception>(() => _weatherRepository.GetCityByNameAsync(cityName));
    }

    [Fact]
    public async Task GetDepartmentByNameAsync_ReturnsNull_WhenDepartmentNotFound()
    {
        var departmentName = "NonExistentDepartment";
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<Department>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(new List<Department>());

        var result = await _weatherRepository.GetDepartmentByNameAsync(departmentName);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetDepartmentByNameAsync_ThrowsException_WhenQueryFails()
    {
        var departmentName = "Paris";
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<Department>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new Exception("Database query failed"));

        await Assert.ThrowsAsync<Exception>(() => _weatherRepository.GetDepartmentByNameAsync(departmentName));
    }

    [Fact]
    public async Task GetWeatherDataByPositionAsync_ReturnsEmptyList_WhenNoNearestStations()
    {
        _mockDapperWrapper.Setup(d => d.QueryAsync<WeatherStation>(_mockConnection.Object, It.IsAny<string>(), null))
            .ReturnsAsync(new List<WeatherStation>());

        var result =
            await _weatherRepository.GetWeatherDataByPositionAsync(0, 0, DateTime.Now, null,
                new List<CategorySearch>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWeatherDataByPositionAsync_ReturnsWeatherData_WhenCategoriesProvided()
    {
        var latitude = 48.8566f;
        var longitude = 2.3522f;
        var startDate = DateTime.Now;
        var categories = new List<CategorySearch> { new() { Name = "Temperature" } };

        var nearestStations = new List<WeatherStation>
        {
            new() { Id = 1, Name = "Station1", Latitude = latitude, Longitude = longitude }
        };
        var expectedWeatherData = new List<WeatherDataResponse>
        {
            new() { Id = 1, time = [startDate], temperature_2m = [20, 40] }
        };

        _mockDapperWrapper.Setup(d => d.QueryAsync<WeatherStation>(_mockConnection.Object, It.IsAny<string>(), null))
            .ReturnsAsync(nearestStations);
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<WeatherDataResponse>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(expectedWeatherData);

        var result =
            await _weatherRepository.GetWeatherDataByPositionAsync(latitude, longitude, startDate, null, categories);

        Assert.Equal(expectedWeatherData, result);
    }

    [Fact]
    public async Task GetWeatherDataByPositionAsync_ReturnsWeatherData_WhenNoCategoriesProvided()
    {
        var latitude = 48.8566f;
        var longitude = 2.3522f;
        var startDate = DateTime.Now;

        var nearestStations = new List<WeatherStation>
        {
            new() { Id = 1, Name = "Station1", Latitude = latitude, Longitude = longitude }
        };
        var expectedWeatherData = new List<WeatherDataResponse>
        {
            new() { Id = 1, time = [startDate], temperature_2m = [20] }
        };

        _mockDapperWrapper.Setup(d => d.QueryAsync<WeatherStation>(_mockConnection.Object, It.IsAny<string>(), null))
            .ReturnsAsync(nearestStations);
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<WeatherDataResponse>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(expectedWeatherData);

        var result = await _weatherRepository.GetWeatherDataByPositionAsync(latitude, longitude, startDate, null, null);

        Assert.Equal(expectedWeatherData, result);
    }

    [Fact]
    public async Task GetWeatherDataByPositionAsync_ThrowsException_WhenQueryFails()
    {
        var latitude = 48.8566f;
        var longitude = 2.3522f;
        var startDate = DateTime.Now;

        var nearestStations = new List<WeatherStation>
        {
            new() { Id = 1, Name = "Station1", Latitude = latitude, Longitude = longitude }
        };

        _mockDapperWrapper.Setup(d => d.QueryAsync<WeatherStation>(_mockConnection.Object, It.IsAny<string>(), null))
            .ReturnsAsync(nearestStations);
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<WeatherDataResponse>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new Exception("Database query failed"));

        await Assert.ThrowsAsync<Exception>(() =>
            _weatherRepository.GetWeatherDataByPositionAsync(latitude, longitude, startDate, null, null));
    }

    [Fact]
    public async Task GetNearestWeatherStationsAsync_ReturnsEmptyList_WhenNoStationsFound()
    {
        _mockDapperWrapper.Setup(d => d.QueryAsync<WeatherStation>(_mockConnection.Object, It.IsAny<string>(), null))
            .ReturnsAsync(new List<WeatherStation>());

        var result = await _weatherRepository.GetNearestWeatherStationsAsync(0, 0, 3);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetNearestWeatherStationsAsync_ReturnsNearestStations()
    {
        var latitude = 48.8566f;
        var longitude = 2.3522f;
        var count = 3;

        var allStations = new List<WeatherStation>
        {
            new() { Id = 1, Name = "Station1", Latitude = latitude, Longitude = longitude },
            new() { Id = 2, Name = "Station2", Latitude = latitude + 0.1f, Longitude = longitude + 0.1f },
            new() { Id = 3, Name = "Station3", Latitude = latitude + 0.2f, Longitude = longitude + 0.2f }
        };

        _mockDapperWrapper.Setup(d => d.QueryAsync<WeatherStation>(_mockConnection.Object, It.IsAny<string>(), null))
            .ReturnsAsync(allStations);

        var result = await _weatherRepository.GetNearestWeatherStationsAsync(latitude, longitude, count);

        Assert.Equal(count, result.Count);
        Assert.Equal("Station1", result[0].Name);
    }

    [Fact]
    public async Task GetNearestWeatherStationsAsync_ThrowsException_WhenQueryFails()
    {
        _mockDapperWrapper.Setup(d => d.QueryAsync<WeatherStation>(_mockConnection.Object, It.IsAny<string>(), null))
            .ThrowsAsync(new Exception("Database query failed"));

        await Assert.ThrowsAsync<Exception>(() =>
            _weatherRepository.GetNearestWeatherStationsAsync(48.8566, 2.3522, 3));
    }

    [Fact]
    public async Task GetGlobalWeatherDataAsync_ReturnsEmptyList_WhenNoDepartments()
    {
        _mockDapperWrapper.Setup(d => d.QueryAsync<Department>(_mockConnection.Object, It.IsAny<string>(), null))
            .ReturnsAsync(new List<Department>());

        var result = await _weatherRepository.GetGlobalWeatherDataAsync(DateTime.Now, null, new List<CategorySearch>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetGlobalWeatherDataAsync_ReturnsWeatherData_WhenCategoriesProvided()
    {
        var startDate = DateTime.Now;
        var categories = new List<CategorySearch> { new() { Name = "Temperature" } };

        var departments = new List<Department>
        {
            new() { Id = "75", Name = "Paris", Latitude = 48.8566f, Longitude = 2.3522f }
        };
        var nearestStations = new List<WeatherStation>
        {
            new() { Id = 1, Name = "Station1", Latitude = 48.8566f, Longitude = 2.3522f }
        };
        var expectedWeatherData = new List<WeatherDataResponse>
        {
            new() { Id = 1, time = [startDate], temperature_2m = [20] }
        };

        _mockDapperWrapper.Setup(d => d.QueryAsync<Department>(_mockConnection.Object, It.IsAny<string>(), null))
            .ReturnsAsync(departments);
        _mockDapperWrapper.Setup(d => d.QueryAsync<WeatherStation>(_mockConnection.Object, It.IsAny<string>(), null))
            .ReturnsAsync(nearestStations);
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<WeatherDataResponse>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(expectedWeatherData);

        var result = await _weatherRepository.GetGlobalWeatherDataAsync(startDate, null, categories);

        Assert.Equal(expectedWeatherData, result);
    }

    [Fact]
    public async Task GetGlobalWeatherDataAsync_ReturnsWeatherData_WhenNoCategoriesProvided()
    {
        var startDate = DateTime.Now;

        var departments = new List<Department>
        {
            new() { Id = "75", Name = "Paris", Latitude = 48.8566f, Longitude = 2.3522f }
        };
        var nearestStations = new List<WeatherStation>
        {
            new() { Id = 1, Name = "Station1", Latitude = 48.8566f, Longitude = 2.3522f }
        };
        var expectedWeatherData = new List<WeatherDataResponse>
        {
            new() { Id = 1, time = [startDate], temperature_2m = [20] }
        };

        _mockDapperWrapper.Setup(d => d.QueryAsync<Department>(_mockConnection.Object, It.IsAny<string>(), null))
            .ReturnsAsync(departments);
        _mockDapperWrapper.Setup(d => d.QueryAsync<WeatherStation>(_mockConnection.Object, It.IsAny<string>(), null))
            .ReturnsAsync(nearestStations);
        _mockDapperWrapper.Setup(d =>
                d.QueryAsync<WeatherDataResponse>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(expectedWeatherData);

        var result = await _weatherRepository.GetGlobalWeatherDataAsync(startDate, null, null);

        Assert.Equal(expectedWeatherData, result);
    }

    [Fact]
    public async Task GetGlobalWeatherDataAsync_ThrowsException_WhenQueryFails()
    {
        var startDate = DateTime.Now;

        var departments = new List<Department>
        {
            new() { Id = "75", Name = "Paris", Latitude = 48.8566f, Longitude = 2.3522f }
        };

        _mockDapperWrapper.Setup(d => d.QueryAsync<Department>(_mockConnection.Object, It.IsAny<string>(), null))
            .ReturnsAsync(departments);
        _mockDapperWrapper.Setup(d => d.QueryAsync<WeatherStation>(_mockConnection.Object, It.IsAny<string>(), null))
            .ThrowsAsync(new Exception("Database query failed"));

        await Assert.ThrowsAsync<Exception>(() => _weatherRepository.GetGlobalWeatherDataAsync(startDate, null, null));
    }
}