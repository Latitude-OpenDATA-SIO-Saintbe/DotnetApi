namespace Metheo.DTO
{
    public class WeatherDataResponse
    {
        public int Id { get; set; }
        public int WeatherStationId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LocationName { get; set; }

        // Hourly Weather Variables
        public float temperature_2m { get; set; }
        public float relative_humidity_2m { get; set; }
        public float dew_point_2m { get; set; }
        public float apparent_temperature { get; set; }
        public float precipitation { get; set; }
        public float rain { get; set; }
        public float snowfall { get; set; }
        public int weather_code { get; set; }
        public float cloud_cover { get; set; }
        public float cloud_cover_low { get; set; }
        public float cloud_cover_mid { get; set; }
        public float cloud_cover_high { get; set; }
        public float pressure_msl { get; set; }
        public float surface_pressure { get; set; }
        public float vapour_pressure_deficit { get; set; }
        public float evapotranspiration { get; set; }
        public float wind_speed_10m { get; set; }
        public float wind_speed_20m { get; set; }
        public float wind_speed_50m { get; set; }
        public float wind_speed_100m { get; set; }
        public float wind_speed_150m { get; set; }
        public float wind_speed_200m { get; set; }
        public float wind_direction_10m { get; set; }
        public float wind_direction_20m { get; set; }
        public float wind_direction_50m { get; set; }
        public float wind_direction_100m { get; set; }
        public float wind_direction_150m { get; set; }
        public float wind_direction_200m { get; set; }
        public float wind_gusts_10m { get; set; }
        public float temperature_20m { get; set; }
        public float temperature_50m { get; set; }
        public float temperature_100m { get; set; }
        public float temperature_150m { get; set; }
        public float temperature_200m { get; set; }
    }
}
