namespace Metheo.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Metheo.Api.Models;
using System.Net;
using Npgsql;
using System.Net.Http;
using System.Text.Json;
using Metheo.Tools;

[ApiController]
[Route("api")]
public class WeatherController : ControllerBase
{
    private readonly WeatherDbContext _context;

    public WeatherController(WeatherDbContext context)
    {
        _context = context;
    }

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
                WHERE table_name = 'WeatherData' AND column_name NOT IN ('Id', 'WeatherStationId', 'Timestamp')", connection))
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
        var nearestStations = await GetNearestStationsIfCitySpecified(city);
        if (!nearestStations.Any() && city != null)
            return NotFound("No nearby weather stations found.");

        var query = _context.WeatherDatas.AsQueryable()
            .Where(wd => wd.Timestamp >= startDate && (endDate == null || wd.Timestamp <= endDate));
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
            var nearestStations = await GetNearestWeatherStations(department.Position, 3);

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
        var weatherData = await query.ToListAsync();
        return weatherData.Any() ? weatherData : null;
    }
    
    private async Task<List<WeatherStation>> GetNearestStationsIfCitySpecified(string? city)
    {
        return city != null ? await GetNearestStations(city) : new List<WeatherStation>();
    }
    
    private IQueryable<WeatherData> ApplyCategoryFilter(IQueryable<WeatherData> query, string? category)
    {
        if (string.IsNullOrEmpty(category)) return query;

        var columnNames = GetWeatherDataColumnNames("WeatherDatas").Result;
        var validCategories = category.Split('&').Where(c => columnNames.Contains(c)).ToList();

        return validCategories.Any() ? query.Where(wd => validCategories.Any(c => EF.Property<string>(wd, c) != null)) : query;
    }

    
    private async Task<List<WeatherData>?> FetchAndStoreApiDataIfNecessary(
        List<WeatherStation> nearestStations, DateTime startDate, DateTime? endDate, string? category)
    {
        if (!nearestStations.Any()) return null;

        var position = nearestStations.First().Position;
        List<WeatherData>? openMeteoData = [await FetchWeatherDataFromApi(startDate, endDate ?? startDate, position, category)];
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
            var cityData = await _context.Cities.FirstOrDefaultAsync(c => c.Name.Equals(city, StringComparison.OrdinalIgnoreCase));
            if (cityData != null)
            {
                nearestStations.UnionWith(await GetNearestWeatherStations(cityData.Position, 3));
            }

            var departmentData = await _context.Departements.FirstOrDefaultAsync(d => d.Name.Equals(city, StringComparison.OrdinalIgnoreCase));
            if (departmentData != null)
            {
                nearestStations.UnionWith(await GetNearestWeatherStations(departmentData.Position, 3));
            }
        }

        return nearestStations.ToList();
    }
    
    // Helper method to get nearest weather stations
    private async Task<List<WeatherStation>> GetNearestWeatherStations(string position, int count)
    {
        var coordinates = Position.RetrievePosition(position);

        var allStations = await _context.WeatherStation.ToListAsync();
        var distances = allStations
            .Select(station => new
            {
                Station = station,
                Distance = Calcul.CalculateHaversineDistance(coordinates.lat, coordinates.lon, station.Position)
            })
            .OrderBy(d => d.Distance)
            .Take(count)
            .Select(d => d.Station)
            .ToList();

        return distances;
    }
    
    private bool IsRecentDateRange(DateTime startDate, DateTime? endDate)
    {
        return startDate >= DateTime.UtcNow.AddDays(-7) ||
               (endDate.HasValue && endDate.Value >= DateTime.UtcNow.AddDays(-7));
    }
    
    private async Task<WeatherData?> FetchWeatherDataFromApi(DateTime startDate, DateTime endDate, string position, string? category = null)
    {
        var apiUrl = Api.GenerateApiUrl(startDate, endDate, position, category);

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

// https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,rain,showers,snowfall,weather_code,cloud_cover,pressure_msl,surface_pressure,wind_speed_10m,wind_direction_10m,wind_gusts_10m&hourly=temperature_2m,relative_humidity_2m,dew_point_2m,apparent_temperature,precipitation,rain,snowfall,weather_code,pressure_msl,surface_pressure,cloud_cover,cloud_cover_low,cloud_cover_mid,cloud_cover_high,et0_fao_evapotranspiration,vapour_pressure_deficit,wind_speed_10m,wind_speed_20m,wind_speed_50m,wind_speed_100m,wind_speed_150m,wind_speed_200m,wind_direction_10m,wind_direction_20m,wind_direction_50m,wind_direction_100m,wind_direction_150m,wind_direction_200m,wind_gusts_10m,temperature_20m,temperature_50m,temperature_100m,temperature_150m,temperature_200m&daily=weather_code,temperature_2m_max,temperature_2m_min,apparent_temperature_max,apparent_temperature_min,sunrise,sunset,daylight_duration,sunshine_duration,uv_index_max,uv_index_clear_sky_max,precipitation_sum,rain_sum,showers_sum,snowfall_sum,precipitation_hours,precipitation_probability_max,wind_speed_10m_max,wind_gusts_10m_max,wind_direction_10m_dominant,shortwave_radiation_sum,et0_fao_evapotranspiration&start_date=2024-10-22&end_date=2024-10-22&models=meteofrance_seamless