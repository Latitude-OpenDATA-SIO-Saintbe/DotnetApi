namespace Metheo.DTO;

public class WeatherDataResponse
{
    public int Id { get; set; }
    public float? WeatherStationLatitude { get; set; }
    public float? WeatherStationLongitude { get; set; }
    public float? Latitude { get; set; }
    public float? Longitude { get; set; }
    public string? ObjectName { get; set; }
    public List<DateTime?> time { get; set; }

// Hourly Weather Variables
    public List<float?> temperature_2m { get; set; }
    public List<float?> relative_humidity_2m { get; set; }
    public List<float?> dew_point_2m { get; set; }
    public List<float?> apparent_temperature { get; set; }
    public List<float?> precipitation { get; set; }
    public List<float?> rain { get; set; }
    public List<float?> snowfall { get; set; }
    public List<int?> weather_code { get; set; }
    public List<float?> cloud_cover { get; set; }
    public List<float?> cloud_cover_low { get; set; }
    public List<float?> cloud_cover_mid { get; set; }
    public List<float?> cloud_cover_high { get; set; }
    public List<float?> pressure_msl { get; set; }
    public List<float?> surface_pressure { get; set; }
    public List<float?> vapour_pressure_deficit { get; set; }
    public List<float?> evapotranspiration { get; set; }
    public List<float?> wind_speed_10m { get; set; }
    public List<float?> wind_speed_20m { get; set; }
    public List<float?> wind_speed_50m { get; set; }
    public List<float?> wind_speed_100m { get; set; }
    public List<float?> wind_speed_150m { get; set; }
    public List<float?> wind_speed_200m { get; set; }
    public List<float?> wind_direction_10m { get; set; }
    public List<float?> wind_direction_20m { get; set; }
    public List<float?> wind_direction_50m { get; set; }
    public List<float?> wind_direction_100m { get; set; }
    public List<float?> wind_direction_150m { get; set; }
    public List<float?> wind_direction_200m { get; set; }
    public List<float?> wind_gusts_10m { get; set; }
    public List<float?> temperature_20m { get; set; }
    public List<float?> temperature_50m { get; set; }
    public List<float?> temperature_100m { get; set; }
    public List<float?> temperature_150m { get; set; }
    public List<float?> temperature_200m { get; set; }
}