namespace Metheo.Api.Models;

public class WeatherStation
{
    // Primary key property - EF Core will use this by default
    public int Id { get; set; }
    public string Name { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
}
