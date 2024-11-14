using Microsoft.VisualBasic.CompilerServices;
namespace Metheo.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Metheo.Api.Models;
using System.Net;
using Npgsql;
using System.Net.Http;
using System.Text.Json;
using Metheo.Tools;

/// <summary>
/// WeatherController provides endpoints to retrieve weather data, city information, and category types.
/// </summary>
[ApiController]
[Route("api")]
public class WeatherController : ControllerBase
{
    private readonly WeatherDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherController"/> class.
    /// </summary>
    /// <param name="context">The database context for weather data.</param>
    public WeatherController(WeatherDbContext context)
    {
        _context = context;
    }
    /// <summary>
    /// Retrieves the list of category types (column names) from the WeatherDatas table.
    /// </summary>
    [HttpGet("categoriestypes")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategoryTypes()
    {
        var columnNames = new List<string>();

        using (var connection = new NpgsqlConnection(_context.Database.GetConnectionString()))
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(@"
                SELECT column_name
                FROM information_schema.columns
                WHERE table_name = 'WeatherDatas' AND column_name NOT IN ('Id', 'WeatherStationId', 'Timestamp')", connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        columnNames.Add(reader.GetString(0));
                    }
                }
            }
        }

        return Ok(columnNames);
    }

    [HttpGet("city")]
    public async Task<ActionResult<IEnumerable<object>>> GetCities()
    {
        var cities = await _context.Cities.ToListAsync();
        var departments = await _context.Departements.ToListAsync();
        var combinedList = new List<object>();
        combinedList.AddRange(cities);
        combinedList.AddRange(departments);
        return Ok(combinedList);
    }

    [HttpGet("{dateRange}/{city?}/{category?}")]
    public async Task<ActionResult<IEnumerable<WeatherData>>> GetWeatherData(string dateRange, string? city = null, string? category = null)
    {
        if (string.IsNullOrWhiteSpace(dateRange))
        {
            return BadRequest("Date range cannot be empty.");
        }

        DateTime startDate;
        DateTime? endDate = null;

        try
        {
            var (start, end) = DateUtils.GetDateRange(dateRange);
            startDate = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            endDate = end.HasValue ? DateTime.SpecifyKind(end.Value, DateTimeKind.Utc) : (DateTime?)null;
        }
        catch (FormatException)
        {
            return BadRequest("Invalid date range format. Use 'YYYY-MM-DD to YYYY-MM-DD'.");
        }

        var query = _context.WeatherDatas.AsQueryable()
            .Where(wd => wd.Timestamp >= startDate &&
                        (endDate == null || wd.Timestamp <= endDate));

        if (city == "France")
        {
            var franceWeatherData = await GetFranceWeatherDataByDepartment(startDate, endDate, category);
            return franceWeatherData.Any() ? Ok(franceWeatherData) : NotFound("No weather data available for France.");
        }

        var result = await GetGlobalWeatherData(city, startDate, endDate, category);
        return result ?? StatusCode((int)HttpStatusCode.InternalServerError, "Error fetching weather data");
    }




























    private async Task<ActionResult<IEnumerable<WeatherData>>> GetGlobalWeatherData(
        string? city, DateTime startDate, DateTime? endDate, string? category)
    {

        var query = _context.WeatherDatas.AsQueryable()
            .Where(wd => wd.Timestamp >= startDate && (endDate == null || wd.Timestamp <= endDate));
        var nearestStations = await GetNearestStationsIfCitySpecified(city);
        if (!nearestStations.Any() && city != null)
            return NotFound("No nearby weather stations found.");
        if (nearestStations.Any())
            query = query.Where(wd => nearestStations.Select(ns => ns.Id).Contains(wd.WeatherStationId));

        query = ApplyCategoryFilter(query, category);

        var weatherData = await GetWeatherDataFromDatabase(query);
        if (weatherData != null)
            return Ok(weatherData);

        var apiWeatherData = await FetchAndStoreApiDataIfNecessary(nearestStations, startDate, endDate, category);
        return apiWeatherData != null ? Ok(apiWeatherData) : null;
    }

    private async Task<IEnumerable<WeatherData>> GetFranceWeatherDataByDepartment(
        DateTime startDate, DateTime? endDate, string? category)
    {
        var franceWeatherData = new List<WeatherData>();
        var departments = await _context.Departements.ToListAsync();

        foreach (var department in departments)
        {
            var nearestStations = await GetNearestWeatherStations(department.Latitude, department.Longitude, 3);

            if (!nearestStations.Any())
                continue;

            // Query weather data for the nearest stations within the date range and category
            var departmentData = _context.WeatherDatas
                .Where(wd => nearestStations.Select(s => s.Id).Contains(wd.WeatherStationId) &&
                             wd.Timestamp >= startDate &&
                             (endDate == null || wd.Timestamp <= endDate));

            // Apply category filter if specified
            departmentData = ApplyCategoryFilter(departmentData, category);

            var weatherData = await GetWeatherDataFromDatabase(departmentData);

            // If data is available, add it to franceWeatherData
            if (weatherData.Any())
            {
                franceWeatherData.AddRange(weatherData);
            }
            else
            {
                // Fetch data from the API if none exists for the current department's stations
                var apiWeatherData = await FetchAndStoreApiDataIfNecessary(nearestStations, startDate, endDate, category);
                if (apiWeatherData != null)
                {
                    franceWeatherData.AddRange(apiWeatherData);
                }
            }
        }

        return franceWeatherData;
    }

    private async Task<List<WeatherData>?> GetWeatherDataFromDatabase(IQueryable<WeatherData> query)
    {
        // console log query
        var queryLog = query.ToString();

        Console.WriteLine("Executing query: {0}", queryLog);
        var weatherData = await query.ToListAsync();
        return weatherData.Any() ? weatherData : null;
    }

    private async Task<List<WeatherStation>> GetNearestStationsIfCitySpecified(string? city)
    {
        return city != null ? await GetNearestStations(city) : new List<WeatherStation>();
    }

    private IQueryable<WeatherData> ApplyCategoryFilter(IQueryable<WeatherData> query, string? category)
    {
        if (string.IsNullOrEmpty(category))
        {
            // No category provided, return the full set of fields
            return query;
        }

        Console.WriteLine("Executing category filter: {0}", category);

        // Get the list of valid categories (column names in the WeatherData table)
        var columnNames = GetWeatherDataColumnNames("WeatherDatas").Result;
        var validCategories = category.Split('&').Where(c => columnNames.Contains(c)).ToList();

        if (validCategories.Any())
        {
            // Start by selecting the common fields (Timestamp, WeatherStationId)
            var projection = query.Select(wd => new
            {
                wd.Timestamp,  // Always include Timestamp
                wd.WeatherStationId, // Always include WeatherStationId

                // Dynamically add properties based on valid categories
                cloud_cover = validCategories.Contains("cloud_cover") ? wd.cloud_cover : 0f,
                temperature_2m = validCategories.Contains("temperature_2m") ? wd.temperature_2m : 0f,
                relative_humidity_2m = validCategories.Contains("relative_humidity_2m") ? wd.relative_humidity_2m : 0f,
                dew_point_2m = validCategories.Contains("dew_point_2m") ? wd.dew_point_2m : 0f,
                precipitation = validCategories.Contains("precipitation") ? wd.precipitation : 0f,
                rain = validCategories.Contains("rain") ? wd.rain : 0f,
                snowfall = validCategories.Contains("snowfall") ? wd.snowfall : 0f,
                weather_code = validCategories.Contains("weather_code") ? wd.weather_code : 0,
                cloud_cover_low = validCategories.Contains("cloud_cover_low") ? wd.cloud_cover_low : 0f,
                cloud_cover_mid = validCategories.Contains("cloud_cover_mid") ? wd.cloud_cover_mid : 0f,
                cloud_cover_high = validCategories.Contains("cloud_cover_high") ? wd.cloud_cover_high : 0f,
                pressure_msl = validCategories.Contains("pressure_msl") ? wd.pressure_msl : 0f,
                surface_pressure = validCategories.Contains("surface_pressure") ? wd.surface_pressure : 0f,
                vapour_pressure_deficit = validCategories.Contains("vapour_pressure_deficit") ? wd.vapour_pressure_deficit : 0f,
                evapotranspiration = validCategories.Contains("evapotranspiration") ? wd.evapotranspiration : 0f,
                wind_speed_10m = validCategories.Contains("wind_speed_10m") ? wd.wind_speed_10m : 0f,
                wind_speed_20m = validCategories.Contains("wind_speed_20m") ? wd.wind_speed_20m : 0f,
                wind_speed_50m = validCategories.Contains("wind_speed_50m") ? wd.wind_speed_50m : 0f,
                wind_speed_100m = validCategories.Contains("wind_speed_100m") ? wd.wind_speed_100m : 0f,
                wind_speed_150m = validCategories.Contains("wind_speed_150m") ? wd.wind_speed_150m : 0f,
                wind_speed_200m = validCategories.Contains("wind_speed_200m") ? wd.wind_speed_200m : 0f,
                wind_direction_10m = validCategories.Contains("wind_direction_10m") ? wd.wind_direction_10m : 0f,
                wind_direction_20m = validCategories.Contains("wind_direction_20m") ? wd.wind_direction_20m : 0f,
                wind_direction_50m = validCategories.Contains("wind_direction_50m") ? wd.wind_direction_50m : 0f,
                wind_direction_100m = validCategories.Contains("wind_direction_100m") ? wd.wind_direction_100m : 0f,
                wind_direction_150m = validCategories.Contains("wind_direction_150m") ? wd.wind_direction_150m : 0f,
                wind_direction_200m = validCategories.Contains("wind_direction_200m") ? wd.wind_direction_200m : 0f,
                wind_gusts_10m = validCategories.Contains("wind_gusts_10m") ? wd.wind_gusts_10m : 0f,
                temperature_20m = validCategories.Contains("temperature_20m") ? wd.temperature_20m : 0f,
                temperature_50m = validCategories.Contains("temperature_50m") ? wd.temperature_50m : 0f,
                temperature_100m = validCategories.Contains("temperature_100m") ? wd.temperature_100m : 0f,
                temperature_150m = validCategories.Contains("temperature_150m") ? wd.temperature_150m : 0f,
                temperature_200m = validCategories.Contains("temperature_200m") ? wd.temperature_200m : 0f
            });

            // Map the anonymous projection back to WeatherData
            query = projection.Select(p => new WeatherData
            {
                Timestamp = p.Timestamp,
                WeatherStationId = p.WeatherStationId,
                cloud_cover = p.cloud_cover,
                temperature_2m = p.temperature_2m,
                relative_humidity_2m = p.relative_humidity_2m,
                dew_point_2m = p.dew_point_2m,
                precipitation = p.precipitation,
                rain = p.rain,
                snowfall = p.snowfall,
                weather_code = p.weather_code,
                cloud_cover_low = p.cloud_cover_low,
                cloud_cover_mid = p.cloud_cover_mid,
                cloud_cover_high = p.cloud_cover_high,
                pressure_msl = p.pressure_msl,
                surface_pressure = p.surface_pressure,
                vapour_pressure_deficit = p.vapour_pressure_deficit,
                evapotranspiration = p.evapotranspiration,
                wind_speed_10m = p.wind_speed_10m,
                wind_speed_20m = p.wind_speed_20m,
                wind_speed_50m = p.wind_speed_50m,
                wind_speed_100m = p.wind_speed_100m,
                wind_speed_150m = p.wind_speed_150m,
                wind_speed_200m = p.wind_speed_200m,
                wind_direction_10m = p.wind_direction_10m,
                wind_direction_20m = p.wind_direction_20m,
                wind_direction_50m = p.wind_direction_50m,
                wind_direction_100m = p.wind_direction_100m,
                wind_direction_150m = p.wind_direction_150m,
                wind_direction_200m = p.wind_direction_200m,
                wind_gusts_10m = p.wind_gusts_10m,
                temperature_20m = p.temperature_20m,
                temperature_50m = p.temperature_50m,
                temperature_100m = p.temperature_100m,
                temperature_150m = p.temperature_150m,
                temperature_200m = p.temperature_200m
            });
        }

        Console.WriteLine("Category query is: {0}", query.ToQueryString());

        return query;
    }

    private async Task<List<WeatherData>?> FetchAndStoreApiDataIfNecessary(
        List<WeatherStation> nearestStations, DateTime startDate, DateTime? endDate, string? category)
    {
        if (!nearestStations.Any()) return null;

        float latitude = nearestStations.First().Latitude;
        float longitude = nearestStations.First().Longitude;
        List<WeatherData>? openMeteoData = [await FetchWeatherDataFromApi(startDate, endDate ?? startDate, latitude, longitude, category)];
        if (openMeteoData == null) return null;

        // Ensure openMeteoData is treated correctly
        List<WeatherData> weatherDataList;

        //if (IsRecentDateRange(startDate, endDate))
        //{
            //var essentialWeatherData = weatherDataList.Select(data => new WeatherData
            //{
                // Map properties from WeatherData to EssentialWeatherData
            //}).ToList();

            //_context.WeatherDatas.AddRange(essentialWeatherData);
            //await _context.SaveChangesAsync();
        //}

        //return weatherDataList; // Return the list of weather data
        return openMeteoData;
    }

    // Get all columns
    private async Task<List<string>> GetWeatherDataColumnNames(string table)
    {
        var columnNames = new List<string>();

        await using (var connection = new NpgsqlConnection(_context.Database.GetConnectionString()))
        {
            await connection.OpenAsync();

            var query = $@"
            SELECT column_name
            FROM information_schema.columns
            WHERE table_name = @table AND column_name NOT IN ('Id', 'WeatherStationId', 'Timestamp')";

            await using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@table", table);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        columnNames.Add(reader.GetString(0));
                    }
                }
            }
        }

        return columnNames;
    }

    private async Task<List<WeatherStation>> GetNearestStations(string? city)
    {
        var nearestStations = new HashSet<WeatherStation>();

        if (!string.IsNullOrWhiteSpace(city))
        {
            // Retrieve city data from the database using Entity Framework
            var cityData = await _context.Cities
                .Where(c => c.Name == city)
                .Select(c => new { c.Latitude, c.Longitude })
                .FirstOrDefaultAsync();

            if (cityData != null)
            {
                nearestStations.UnionWith(await GetNearestWeatherStations(cityData.Latitude, cityData.Longitude, 3));
            }

            // Retrieve department data from the database using Entity Framework
            var departmentData = await _context.Departements
                .Where(d => d.Name == city)
                .Select(d => new { d.Latitude, d.Longitude })
                .FirstOrDefaultAsync();

            if (departmentData != null)
            {
                nearestStations.UnionWith(await GetNearestWeatherStations(departmentData.Latitude, departmentData.Longitude, 3));
            }
        }

        return nearestStations.ToList();
    }

    // Helper method to get nearest weather stations
    private async Task<List<WeatherStation>> GetNearestWeatherStations(float Latitude, float Longitude, int count)
    {
        var coordinates = Position.RetrievePosition(Latitude, Longitude);

        var allStations = await _context.WeatherStation.ToListAsync();
        var distances = allStations
            .Select(station => new
            {
                Station = station,
                Distance = Calcul.CalculateHaversineDistance(coordinates.lat, coordinates.lon, station.Latitude, station.Longitude)
            })
            .OrderBy(d => d.Distance)
            .Take(count)
            .Select(d => d.Station)
            .ToList();

        // console log the return
        Console.WriteLine("Nearest weather stations: {0}", string.Join(", ", distances.Select(s => s.Id)));

        return distances;
    }

    private bool IsRecentDateRange(DateTime startDate, DateTime? endDate)
    {
        return startDate >= DateTime.UtcNow.AddDays(-7) ||
               (endDate.HasValue && endDate.Value >= DateTime.UtcNow.AddDays(-7));
    }

    private async Task<WeatherData?> FetchWeatherDataFromApi(DateTime startDate, DateTime endDate, float latitude, float longitude, string? category = null)
    {
        var apiUrl = Api.GenerateApiUrl(startDate, endDate, latitude, longitude, category);

        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(apiUrl);
        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<WeatherData>(jsonResponse);
        }
        return null;
    }
}
