namespace Metheo.Tools;

/// <summary>
/// Provides methods to interact with the API.
/// </summary>
public static class Api
{
    /// <summary>
    /// Generates the API URL with the specified parameters.
    /// </summary>
    /// <param name="startDate">The start date for the forecast in yyyy-MM-dd format.</param>
    /// <param name="endDate">The end date for the forecast in yyyy-MM-dd format.</param>
    /// <param name="latitude">The latitude coordinate for the location.</param>
    /// <param name="longitude">The longitude coordinate for the location.</param>
    /// <param name="category">An optional category parameter for the API.</param>
    /// <returns>A string representing the generated API URL.</returns>
    public static string GenerateApiUrl(DateTime startDate, DateTime endDate, float latitude, float longitude, string? category = null)
    {
        var baseUrl = "https://api.open-meteo.com/v1/forecast?";
        var queryParams = new List<string>
        {
            $"start_date={startDate:yyyy-MM-dd}",
            $"end_date={endDate:yyyy-MM-dd}",
        };

        var coordinates = Position.RetrievePosition(latitude, longitude);
        queryParams.Add($"latitude={coordinates.lat:F4}");
        queryParams.Add($"longitude={coordinates.lon:F4}");

        if (!string.IsNullOrEmpty(category))
        {
            queryParams.Add($"{category}");
        }

        return baseUrl + string.Join("&", queryParams);
    }

}
