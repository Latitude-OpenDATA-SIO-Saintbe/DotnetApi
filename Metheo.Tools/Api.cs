using System.Text.Json;
using Metheo.DTO;

namespace Metheo.Tools;

/// <summary>
///     Provides methods to interact with the API.
/// </summary>
public static class Api
{
    /// <summary>
    ///     Generates the API URL with the specified parameters.
    /// </summary>
    /// <param name="startDate">The start date for the forecast in yyyy-MM-dd format.</param>
    /// <param name="endDate">The end date for the forecast in yyyy-MM-dd format.</param>
    /// <param name="latitude">The latitude coordinate for the location.</param>
    /// <param name="longitude">The longitude coordinate for the location.</param>
    /// <param name="category">An optional category parameter for the API.</param>
    /// <returns>A string representing the generated API URL.</returns>
    public static string GenerateApiUrl(DateTime startDate, DateTime endDate, float latitude, float longitude, List<CategorySearch> category = null)
    {
        var baseUrl = "https://api.open-meteo.com/v1/forecast?";
        var queryParams = new List<string>
        {
            $"start_date={startDate:yyyy-MM-dd}",
            $"end_date={endDate:yyyy-MM-dd}"
        };

        var coordinates = Position.RetrievePosition(latitude, longitude);
        queryParams.Add($"latitude={coordinates.lat:F4}");
        queryParams.Add($"longitude={coordinates.lon:F4}");

        if (category != null && category.Any())
        {
            var categoryNames = string.Join(",", category.Select(c => c.Name));
            queryParams.Add($"hourly={categoryNames}");
        }

        return baseUrl + string.Join("&", queryParams);
    }

    /// <summary>
    ///    Fetches weather data from the API.
    /// </summary>
    /// <param name="url">The URL to fetch the weather data from.</param>
    /// <returns>A list of weather data responses.</returns>
    /// <exception cref="HttpRequestException">Thrown when an error occurs while fetching the data.</exception>
    /// <exception cref="JsonException">Thrown when an error occurs while parsing the JSON response.</exception>
    /// <exception cref="Exception">Thrown when an error occurs while processing the data.</exception>
    /// <exception cref="ArgumentException">Thrown when the URL is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the URL is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the URL is empty.</exception>
    /// <exception cref="NotSupportedException">Thrown when the URL scheme is not supported.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the request is disposed.</exception>
    public static async Task<List<WeatherDataResponse>> GetWeatherDataAsync(string url)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be null or empty.");

        using var client = new HttpClient();
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to fetch data from the API. Status code: {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();
        var weatherData = JsonSerializer.Deserialize<List<WeatherDataResponse>>(content);

        return weatherData ?? throw new Exception("Failed to parse the weather data.");
    }
}