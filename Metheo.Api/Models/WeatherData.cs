using System;

namespace Metheo.Api.Models
{
    public class WeatherData
    {
        public int Id { get; set; }
        public int WeatherStationId { get; set; }
        public DateTime Timestamp { get; set; }
        
        public float? Current_temperature_2m { get; set; }
        public float? Current_relative_humidity_2m { get; set; }
        public float? Current_apparent_temperature { get; set; }
        public bool? Current_is_day { get; set; }
        public float? Current_precipitation { get; set; }
        public float? Current_rain { get; set; }
        public float? Current_showers { get; set; }
        public float? Current_snowfall { get; set; }
        public int? Current_weather_code { get; set; }
        public float? Current_cloud_cover { get; set; }
        public float? Current_pressure_msl { get; set; }
        public float? Current_surface_pressure { get; set; }
        public float? Current_wind_speed_10m { get; set; }
        public float? Current_wind_direction_10m { get; set; }
        public float? Current_wind_gusts_10m { get; set; }

        // Hourly Weather Variables
        public float? Hourly_temperature_2m { get; set; }
        public float? Hourly_relative_humidity_2m { get; set; }
        public float? Hourly_dew_point_2m { get; set; }
        public float? Hourly_apparent_temperature { get; set; }
        public float? Hourly_precipitation { get; set; }
        public float? Hourly_rain { get; set; }
        public float? Hourly_snowfall { get; set; }
        public int? Hourly_weather_code { get; set; }
        public float? Hourly_cloud_cover_total { get; set; }
        public float? Hourly_cloud_cover_low { get; set; }
        public float? Hourly_cloud_cover_mid { get; set; }
        public float? Hourly_cloud_cover_high { get; set; }
        public float? Hourly_pressure_msl { get; set; }
        public float? Hourly_surface_pressure { get; set; }
        public float? Hourly_vapour_pressure_deficit { get; set; }
        public float? Hourly_reference_evapotranspiration { get; set; }
        public float? Hourly_wind_speed_10m { get; set; }
        public float? Hourly_wind_speed_20m { get; set; }
        public float? Hourly_wind_speed_50m { get; set; }
        public float? Hourly_wind_speed_100m { get; set; }
        public float? Hourly_wind_speed_150m { get; set; }
        public float? Hourly_wind_speed_200m { get; set; }
        public float? Hourly_wind_direction_10m { get; set; }
        public float? Hourly_wind_direction_20m { get; set; }
        public float? Hourly_wind_direction_50m { get; set; }
        public float? Hourly_wind_direction_100m { get; set; }
        public float? Hourly_wind_direction_150m { get; set; }
        public float? Hourly_wind_direction_200m { get; set; }
        public float? Hourly_wind_gusts_10m { get; set; }
        public float? Hourly_temperature_20m { get; set; }
        public float? Hourly_temperature_50m { get; set; }
        public float? Hourly_temperature_100m { get; set; }
        public float? Hourly_temperature_150m { get; set; }
        public float? Hourly_temperature_200m { get; set; }

        // Daily Weather Variables
        public int? Daily_weather_code { get; set; }
        public float? Daily_max_temperature_2m { get; set; }
        public float? Daily_min_temperature_2m { get; set; }
        public float? Daily_max_apparent_temperature { get; set; }
        public float? Daily_min_apparent_temperature { get; set; }
        public DateTime? Daily_sunrise { get; set; }
        public DateTime? Daily_sunset { get; set; }
        public int? Daily_daylight_duration { get; set; }
        public int? Daily_sunshine_duration { get; set; }
        public float? Daily_uv_index { get; set; }
        public float? Daily_uv_index_clear_sky { get; set; }
        public float? Daily_precipitation_sum { get; set; }
        public float? Daily_rain_sum { get; set; }
        public float? Daily_showers_sum { get; set; }
        public float? Daily_snowfall_sum { get; set; }
        public int? Daily_precipitation_hours { get; set; }
        public float? Daily_precipitation_probability_max { get; set; }
        public float? Daily_max_wind_speed_10m { get; set; }
        public float? Daily_max_wind_gusts_10m { get; set; }
        public float? Daily_dominant_wind_direction_10m { get; set; }
        public float? Daily_shortwave_radiation_sum { get; set; }
        public float? Daily_reference_evapotranspiration { get; set; }
    }
}
