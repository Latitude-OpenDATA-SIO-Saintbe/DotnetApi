using Metheo.DAL;
using Metheo.DTO;
using Metheo.Tools;
using Microsoft.Extensions.Caching.Memory;

namespace Metheo.BL;

public interface IWeatherService
{
    Task<List<CategoryType>?> GetCategoryTypesAsync();
    Task<List<object>> PostCitiesAndDepartmentsAsync(string searchCity);
    Task<LocationDto?> GetCityOrDepartmentPositionAsync(string city);

    Task<IEnumerable<WeatherDataResponse>> GetWeatherData(string dateRange, string latitude, string longitude,
        string? category);
}

public class WeatherService : IWeatherService
{
    private readonly IMemoryCache _cache;

    private readonly IWeatherRepository _repository;

    public WeatherService(IWeatherRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<CategoryType>?> GetCategoryTypesAsync()
    {
        const string cacheKey = "CategoryTypes";

        // Check if categories are cached
        if (_cache.TryGetValue(cacheKey, out List<CategoryType>? cachedCategories)) return cachedCategories;

        // Fetch categories from the repository
        var columnNames = await _repository.GetCategoryTypesAsync();
        if (columnNames == null) throw new Exception("No category name found in the database.");

        // Map to CategoryType objects
        var categories = columnNames.Select(c => new CategoryType { Name = c }).ToList();

        _cache.Set(cacheKey, categories, TimeSpan.FromDays(10)); // Cache for 10 days

        return categories;
    }

    public async Task<List<object>> PostCitiesAndDepartmentsAsync(string searchCity)
    {
        var cities = await _repository.GetCitiesAsync(searchCity);
        var departments = await _repository.GetDepartmentsAsync(searchCity);

        var combinedList = new List<object>();
        combinedList.AddRange(cities.Select(c => new { c.Id, c.Name }));
        combinedList.AddRange(departments.Select(d => new { d.Id, d.Name }));

        return combinedList;
    }

    public async Task<LocationDto?> GetCityOrDepartmentPositionAsync(string city)
    {
        var cityPosition = await _repository.GetCityByNameAsync(city);
        if (cityPosition != null)
            return new LocationDto { Latitude = cityPosition.Latitude, Longitude = cityPosition.Longitude };

        var departmentPosition = await _repository.GetDepartmentByNameAsync(city);
        return departmentPosition != null
            ? new LocationDto { Latitude = departmentPosition.Latitude, Longitude = departmentPosition.Longitude }
            : null; // Return null if not found
    }

    public async Task<IEnumerable<WeatherDataResponse>> GetWeatherData(string dateRange, string latitude, string longitude,
        string? category)
    {
        if (string.IsNullOrWhiteSpace(dateRange))
            throw new ArgumentException("Date range cannot be empty. Please provide a valid range in the format 'YYYY-MM-DD:YYYY-MM-DD'.");

        if (!DateUtils.TryParseDateRange(dateRange, out var startDate, out var endDate))
            throw new ArgumentException($"Invalid date range format: {dateRange}. Expected format is 'YYYY-MM-DD:YYYY-MM-DD'.");

        var categories = new List<CategorySearch>();

        if (category is not null)
            categories = await GetCategoryName(category);

        if (!double.TryParse(latitude, out var lat) || !double.TryParse(longitude, out var lon))
        {
            if (latitude.ToLower() == "france" && longitude.ToLower() == "france")
            {
                const string globalWeatherCacheKey = "GlobalWeatherData";

                if (_cache.TryGetValue(globalWeatherCacheKey, out List<WeatherDataResponse>? cachedGlobalWeatherData))
                    return cachedGlobalWeatherData;

                var globalWeatherData = await _repository.GetGlobalWeatherDataAsync(startDate, endDate, categories);
                if (!globalWeatherData.Any())
                    throw new Exception("No weather data found for the specified criteria.");

                _cache.Set(globalWeatherCacheKey, globalWeatherData, TimeSpan.FromMinutes(50));
                return globalWeatherData;
            }

            throw new ArgumentException($"Invalid latitude or longitude values: ({latitude}, {longitude}).");
        }

        const string weatherCacheKey = "WeatherData";

        if (_cache.TryGetValue(weatherCacheKey, out List<WeatherDataResponse>? cachedWeatherData))
            return cachedWeatherData;

        var weatherData = await _repository.GetWeatherDataByPositionAsync(lat, lon, startDate, endDate, categories);

        if (!weatherData.Any())
        {
            try
            {
                var apiUrl = Api.GenerateApiUrl(startDate, (DateTime)endDate, (float)lat, (float)lon, categories);
                var apiWeatherData = await Api.GetWeatherDataAsync(apiUrl);

                if (apiWeatherData.Any())
                {
                    weatherData = apiWeatherData;
                }
                else
                    throw new Exception("No weather data found for the specified criteria.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        _cache.Set(weatherCacheKey, weatherData, TimeSpan.FromMinutes(5));

        return weatherData;
    }

    // add category Name
    public async Task<List<CategorySearch>> GetCategoryName(string categoryName)
    {
        var categoryDb = await GetCategoryTypesAsync();
        var categoryDict = categoryDb!.ToDictionary(c => c.Name);
        var categories = categoryName.Split('&').Select(c => new CategorySearch { Name = c }).ToList();
        // CategoryName is like (pluie&vent) or (pluie&vent&temperature)
        // and we need to check if the category name is in the database
        // if not return a bad request else return a list of all category name like [{name: "pluie"}, {name: "vent"}] or [{name: "pluie"}, {name: "vent"}, {name: "temperature"}]

        foreach (var category in categories)
            if (!categoryDict.ContainsKey(category.Name))
                throw new Exception($"Category name '{category.Name}' is not in the database.");

        return categories;
    }
}