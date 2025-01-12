using Metheo.DTO;
using Dapper;
using System.Data;
using Metheo.Tools;

namespace Metheo.DAL
{
    public interface IWeatherRepository
    {
        Task<List<string>> GetCategoryTypesAsync();
        Task<List<Cities>> GetCitiesAsync(string searchCity);
        Task<List<Department>> GetDepartmentsAsync(string searchCity);
        Task<Cities?> GetCityByNameAsync(string cityName);
        Task<Department?> GetDepartmentByNameAsync(string departmentName);
        Task<List<WeatherDataResponse>> GetWeatherDataByPositionAsync(double latitude, double longitude, DateTime startDate,
            DateTime? endDate, List<CategorySearch> category);
        Task<List<WeatherDataResponse>> GetGlobalWeatherDataAsync(DateTime startDate, DateTime? endDate,
            List<CategorySearch> category);
    }

    public class WeatherRepository : IWeatherRepository
    {
        private readonly IDbConnection _connection;

        // Constructor to inject the connection (e.g., PostgreSQL)
        public WeatherRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<string>> GetCategoryTypesAsync()
        {
            var query = """
                        
                                        SELECT column_name
                                        FROM information_schema.columns
                                        WHERE table_name = 'WeatherDatas' AND column_name NOT IN ('Id', 'WeatherStationId', 'Timestamp')
                        """;

            var result = await _connection.QueryAsync<string>(query);

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

            var result = await _connection.QueryAsync<Cities>(query, new { SearchCity = $"%{searchCity}%" });

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

            var result = await _connection.QueryAsync<Department>(query, new { SearchCity = $"%{searchCity}%" });

            return result.ToList(); // Maps results to a list of Department DTOs
        }

        public async Task<Cities?> GetCityByNameAsync(string cityName)
        {
            var query = """
                        
                                        SELECT "Id", "Name", "Latitude", "Longitude"
                                        FROM "Cities"
                                        WHERE "Name" = @CityName
                        """;

            var result = await _connection.QueryFirstOrDefaultAsync<Cities>(query, new { CityName = cityName });

            return result; // Returns a City object or null if not found
        }
        
        public async Task<List<Department>> GetDepartmentPositionAsync()
        {
            var query = """
                        
                                        SELECT "Numero" AS "Id", "Name", "Latitude", "Longitude"
                                        FROM "Departements"
                        """;

            var result = await _connection.QueryAsync<Department>(query);

            return result.ToList(); // Returns a list of Department objects with their positions
        }

        public async Task<Department?> GetDepartmentByNameAsync(string departmentName)
        {
            var query = """
                        
                                        SELECT "Id", "Numero", "Name", "Latitude", "Longitude"
                                        FROM "Departements"
                                        WHERE "Name" = @DepartmentName
                        """;

            var result =
                await _connection.QueryFirstOrDefaultAsync<Department>(query, new { DepartmentName = departmentName });

            return result; // Returns a Department object or null if not found
        }

        public async Task<List<WeatherDataResponse>> GetWeatherDataByPositionAsync(double latitude, double longitude,
            DateTime startDate, DateTime? endDate, List<CategorySearch>? categories)
        {
            var nearestStations = await GetNearestWeatherStationsAsync(latitude, longitude, 3);
            if (!nearestStations.Any())
                return new List<WeatherDataResponse>();

            var stationIds = nearestStations.Select(ns => ns.Id);

            // Build the SQL query based on whether categories are provided
            string query;
            if (categories == null || !categories.Any())
            {
                // If no categories, select all columns along with station details
                query = """
                            SELECT wd.*, 
                                   cs."Latitude" AS "LocationLatitude", 
                                   cs."Longitude" AS "LocationLongitude", 
                                   cs."Name" AS "LocationName"
                            FROM "WeatherDatas" wd
                            JOIN "Cities" cs ON cs."Id" = wd."WeatherStationId"
                            WHERE "WeatherStationId" = ANY(@StationIds)
                              AND "Timestamp" >= @StartDate
                              AND (@EndDate IS NULL OR "Timestamp" <= @EndDate)
                        """;
            }
            else
            {
                // If categories are provided, filter by valid category names and select only those columns
                var categoryNames = categories.Select(c => c.Name).ToArray();
                query = $@"
                            SELECT wd.{string.Join(", wd.", categoryNames)}, 
                                   wd.""Timestamp"", wd.""WeatherStationId"", wd.""Id"",
                                   cs.""Latitude"" AS ""LocationLatitude"", 
                                   cs.""Longitude"" AS ""LocationLongitude"", 
                                   cs.""Name"" AS ""LocationName""
                            FROM ""WeatherDatas"" wd
                            JOIN ""Cities"" cs ON cs.""Id"" = wd.""WeatherStationId""
                            WHERE ""WeatherStationId"" = ANY(@StationIds)
                              AND ""Timestamp"" >= @StartDate
                              AND (@EndDate IS NULL OR ""Timestamp"" <= @EndDate)
                        ";
            }

            var parameters = new 
            { 
                StationIds = stationIds.ToArray(), 
                StartDate = startDate, 
                EndDate = endDate,
            };
            var result = await _connection.QueryAsync<WeatherDataResponse>(query, parameters);
            return result.ToList();
        }

        public async Task<List<WeatherStation>> GetNearestWeatherStationsAsync(double latitude, double longitude, int count)
        {
            var allStationsQuery = """
                                   
                                               SELECT "Id", "Name", "Latitude", "Longitude"
                                               FROM "WeatherStation"
                                   """;

            var allStations = (await _connection.QueryAsync<WeatherStation>(allStationsQuery)).ToList();

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
        
        // Get global weather data for France each departement return wheather data of the 3 closest weather station (departement is get by GetDepartmentPositionAsync)
        public async Task<List<WeatherDataResponse>> GetGlobalWeatherDataAsync(DateTime startDate, DateTime? endDate,
            List<CategorySearch> categories)
        {
            var departments = await GetDepartmentPositionAsync();
            if (!departments.Any())
                return new List<WeatherDataResponse>();

            var departmentIds = departments.Select(d => d.Id);
            var allResults = new List<WeatherDataResponse>();

            foreach (var id in departmentIds)
            {
                // Get nearest stations for each department
                var nearestStations = await GetNearestWeatherStationsAsync(departments.First(d => d.Id == id).Latitude,
                    departments.First(d => d.Id == id).Longitude, 3);

                // Build query with dynamic category selection
                string query;
                if (categories == null || !categories.Any())
                {
                    // If no categories, select all columns along with department details
                    query = """
                                SELECT wd.*, 
                                       d."Latitude" AS "LocationLatitude", 
                                       d."Longitude" AS "LocationLongitude", 
                                       d."Name" AS "LocationName"
                                FROM "WeatherDatas" wd
                                JOIN "Departements" d
                                JOIN "Cities" cs
                                WHERE "WeatherStationId" = ANY(@StationIds)
                                  AND "Timestamp" >= @StartDate
                                  AND (@EndDate IS NULL OR "Timestamp" <= @EndDate)
                            """;
                }
                else
                {
                    // If categories are provided, filter by valid category names and select only those columns
                    var categoryNames = categories.Select(c => c.Name).ToArray();
                    query = $@"
                                SELECT wd.{string.Join(", wd.", categoryNames)}, 
                                       wd.""Timestamp"", wd.""WeatherStationId"", wd.""Id"",
                                       d.""Latitude"" AS ""LocationLatitude"", 
                                       d.""Longitude"" AS ""LocationLongitude"", 
                                       d.""Name"" AS ""LocationName""
                                FROM ""WeatherDatas"" wd
                                JOIN ""Departements"" d
                                JOIN ""Cities"" cs
                                WHERE ""WeatherStationId"" = ANY(@StationIds)
                                  AND ""Timestamp"" >= @StartDate
                                  AND (@EndDate IS NULL OR ""Timestamp"" <= @EndDate)
                            ";
                }

                var parameters = new 
                { 
                    StationIds = nearestStations.Select(ns => ns.Id).ToArray(), 
                    StartDate = startDate, 
                    EndDate = endDate,
                };
                var result = await _connection.QueryAsync<WeatherDataResponse>(query, parameters);
                allResults.AddRange(result);
            }

            return allResults;
        }
    }
}
