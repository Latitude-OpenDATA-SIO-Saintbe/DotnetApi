namespace Metheo.Tools;

public class Api
{
    // Helper method to generate the API URL
    public static string GenerateApiUrl(DateTime startDate, DateTime endDate, string position, string? category = null)
    {
        var baseUrl = "https://api.open-meteo.com/v1/forecast?";
        var queryParams = new List<string>
        {
            $"start_date={startDate:yyyy-MM-dd}",
            $"end_date={endDate:yyyy-MM-dd}",
        };
        
        var coordinates = Position.RetrievePosition(position);
        queryParams.Add($"latitude={coordinates.lat}");
        queryParams.Add($"longitude={coordinates.lon}");

        if (!string.IsNullOrEmpty(category))
        {
            queryParams.Add($"{category}");
        }

        return baseUrl + string.Join("&", queryParams);
    }

}