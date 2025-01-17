using System.Data;
using Autofac;
using Metheo.DTO;
using Metheo.Tools;
using Microsoft.AspNetCore.Mvc;

namespace Metheo.DAL;

public interface IWeatherRepository
{
    Task<List<string>> GetCategoryTypesAsync();
    Task<List<Cities>> GetCitiesAsync(string searchCity);
    Task<List<Department>> GetDepartmentsAsync(string searchCity);
    Task<Cities?> GetCityByNameAsync(string cityName);
    Task<Department?> GetDepartmentByNameAsync(string departmentName);

    Task<List<WeatherDataResponse>> GetWeatherDataByPositionAsync(double latitude, double longitude,
        DateTime startDate,
        DateTime? endDate, List<CategorySearch> category);

    Task<List<WeatherDataResponse>> GetGlobalWeatherDataAsync(DateTime startDate, DateTime? endDate,
        List<CategorySearch> category);
}

public class WeatherRepository : IWeatherRepository
{
    private readonly IDbConnection _postgresConnection;
    private readonly IDapperWrapper _dapperWrapper;

    // Constructor to inject the connection (e.g., PostgreSQL)
    public WeatherRepository(IComponentContext context, IDapperWrapper dapperWrapper)
    {
        _postgresConnection = context.ResolveNamed<IDbConnection>("PostgresConnection");
        ;
        _dapperWrapper = dapperWrapper;

        Console.WriteLine($"Connected to database: {_postgresConnection.Database}");
    }

    public async Task<List<string>> GetCategoryTypesAsync()
    {
        var query = """
                    SELECT column_name
                    FROM information_schema.columns
                    WHERE table_name = 'WeatherDatas' AND column_name NOT IN ('Id', 'WeatherStationId', 'Timestamp')
                    """;

        var result = await _dapperWrapper.QueryAsync<string>(_postgresConnection, query);

        return result.ToList(); // Returns a List of strings (column names)
    }

    public async Task<List<Cities>> GetCitiesAsync(string searchCity)
    {
        var query = """
                    SELECT "Id", "Name", "Latitude", "Longitude"
                    FROM "Cities"
                    WHERE "Name" LIKE @SearchCity
                    LIMIT 5
                    """;

        var result =
            await _dapperWrapper.QueryAsync<Cities>(_postgresConnection, query, new { SearchCity = $"%{searchCity}%" });

        return result.ToList(); // Returns a list of City DTOs
    }

    public async Task<List<Department>> GetDepartmentsAsync(string searchCity)
    {
        var query = """
                    SELECT "Numero" AS "Id", "Name", "Latitude", "Longitude"
                    FROM "Departements"
                    WHERE "Name" LIKE @SearchCity
                    LIMIT 2
                    """;

        var result =
            await _dapperWrapper.QueryAsync<Department>(_postgresConnection, query,
                new { SearchCity = $"%{searchCity}%" });

        return result.ToList(); // Maps results to a list of Department DTOs
    }

    public async Task<Cities?> GetCityByNameAsync(string cityName)
    {
        var query = """
                    SELECT "Id", "Name", "Latitude", "Longitude"
                    FROM "Cities"
                    WHERE "Name" = @CityName
                    """;

        var result = await _dapperWrapper.QueryAsync<Cities>(_postgresConnection, query, new { CityName = cityName });

        return result.FirstOrDefault(); // Returns a City object or null if not found
    }

    public async Task<Department?> GetDepartmentByNameAsync(string departmentName)
    {
        var query = """
                    SELECT "Id", "Numero", "Name", "Latitude", "Longitude"
                    FROM "Departements"
                    WHERE "Name" = @DepartmentName
                    """;

        var result =
            await _dapperWrapper.QueryAsync<Department>(_postgresConnection, query,
                new { DepartmentName = departmentName });

        return result.FirstOrDefault(); // Returns a Department object or null if not found
    }

    public async Task<List<WeatherDataResponse>> GetWeatherDataByPositionAsync(double latitude, double longitude,
        DateTime startDate, DateTime? endDate, List<CategorySearch> categories)
    {
        var nearestStations = await GetNearestWeatherStationsAsync(latitude, longitude, 3);
        if (!nearestStations.Any())
            return new List<WeatherDataResponse>();

        var stationIds = nearestStations.Select(ns => ns.Id);
        var categoryNames = categories?.Select(c => c.Name).ToArray();

        if (categories == null || !categories.Any())
        {
            categoryNames = (await GetCategoryTypesAsync()).ToArray();
        }

        // Dynamically create json_agg for each category
        string categoryColumns = string.Join(", ", categoryNames.Select(c => $"json_agg(wd.\"{c}\") AS \"{c}\""));

        // Construct the SQL query
        var query = $"""
                         SELECT json_agg(wd."Timestamp") AS "time",
                                {categoryColumns},
                                wd."WeatherStationId" as "Id",
                                ws."Latitude" AS "WeatherStationLatitude",
                                ws."Longitude" AS "WeatherStationLongitude"
                         FROM "WeatherDatas" wd
                         JOIN "WeatherStation" ws ON ws."Id" = wd."WeatherStationId"
                         WHERE wd."WeatherStationId" = ANY(@StationIds)
                           AND wd."Timestamp" >= @StartDate
                           AND (@EndDate IS NULL OR wd."Timestamp" <= @EndDate)
                         GROUP BY ws."Latitude", ws."Longitude", wd."WeatherStationId"
                     """;

        var parameters = new
        {
            StationIds = stationIds.ToArray(),
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _dapperWrapper.QueryAsync<WeatherDataResponse>(_postgresConnection, query, parameters);
        return result.ToList();
    }


    // Get global weather data for France each departement return wheather data of the 3 closest weather station (departement is get by GetDepartmentPositionAsync)
    public async Task<List<WeatherDataResponse>> GetGlobalWeatherDataAsync(DateTime startDate, DateTime? endDate,
        List<CategorySearch> categories)
    {
        var departments = await GetDepartmentPositionAsync();
        if (!departments.Any())
            return new List<WeatherDataResponse>();

        var departmentIds = departments.Select(d => d.Id);
        var allResults = new List<WeatherDataResponse>();

        string[] categoryNames = categories?.Select(c => c.Name).ToArray() ?? Array.Empty<string>();

        foreach (var id in departmentIds)
        {
            var nearestStations = await GetNearestWeatherStationsAsync(departments.First(d => d.Id == id).Latitude,
                departments.First(d => d.Id == id).Longitude, 3);

            if (!categoryNames.Any())
            {
                categoryNames = (await GetCategoryTypesAsync()).ToArray();
            }

            var categoryColumns = string.Join(", ", categoryNames.Select(c => $"json_agg(wd.\"{c}\") AS \"{c}\""));
            var query = $"""
                             SELECT json_agg(wd."Timestamp") AS "time",
                                    {categoryColumns},
                                    wd."WeatherStationId" as "Id",
                                    ws."Latitude" AS "WeatherStationLatitude",
                                    ws."Longitude" AS "WeatherStationLongitude",
                                    d."Numero" AS "ObjectName",
                                    d."Latitude" AS "Latitude",
                                    d."Longitude" AS "Longitude"
                             FROM "WeatherDatas" wd
                             JOIN "WeatherStation" ws ON ws."Id" = wd."WeatherStationId"
                             JOIN "Departements" d ON d."Numero" = @DepartmentId::text
                             WHERE wd."WeatherStationId" = ANY(@StationIds)
                               AND wd."Timestamp" >= @StartDate
                               AND (@EndDate IS NULL OR wd."Timestamp" <= @EndDate)
                             GROUP BY ws."Latitude", ws."Longitude", wd."WeatherStationId", d."Latitude", d."Longitude", d."Numero"
                         """;

            var parameters = new
            {
                StationIds = nearestStations.Select(ns => ns.Id).ToArray(),
                StartDate = startDate,
                EndDate = endDate,
                DepartmentId = id
            };
            var result = await _dapperWrapper.QueryAsync<WeatherDataResponse>(_postgresConnection, query, parameters);
            allResults.AddRange(result);
        }

        return allResults;
    }

    public async Task<List<Department>> GetDepartmentPositionAsync()
    {
        var query = """
                    SELECT "Numero" AS "Id", "Name", "Latitude", "Longitude"
                    FROM "Departements"
                    """;

        var result = await _dapperWrapper.QueryAsync<Department>(_postgresConnection, query);

        return result.ToList(); // Returns a list of Department objects with their positions
    }

    public async Task<List<WeatherStation>> GetNearestWeatherStationsAsync(double latitude, double longitude, int count)
    {
        var allStationsQuery = """
                               SELECT "Id", "Name", "Latitude", "Longitude"
                               FROM "WeatherStation"
                               """;

        var allStations = (await _dapperWrapper.QueryAsync<WeatherStation>(_postgresConnection, allStationsQuery))
            .ToList();

        return allStations
            .Select(station => new
            {
                Station = station,
                Distance = Calcul.CalculateHaversineDistance(latitude, longitude, station.Latitude,
                    station.Longitude)
            })
            .OrderBy(d => d.Distance)
            .Take(count)
            .Select(d => d.Station)
            .ToList();
    }
}