using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using Metheo.BL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiDotnetMetheoOrm.Controller;

[Route("api/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [Authorize(Policy = "CanViewData")]
    [HttpGet("categoriestypes")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategoryTypes()
    {
        var categoryTypes = await _weatherService.GetCategoryTypesAsync();
        return Ok(categoryTypes.Select(c => c.Name)); // Just column names
    }

    [Authorize(Policy = "CanViewData")]
    [HttpGet("search/{searchCity}")]
    public async Task<ActionResult<IEnumerable<object>>> PostCities(string searchCity)
    {
        var citiesAndDepartments = await _weatherService.PostCitiesAndDepartmentsAsync(searchCity);
        return Ok(citiesAndDepartments);
    }

    [Authorize(Policy = "CanViewData")]
    [HttpGet("city/{city}")]
    public async Task<ActionResult<object>> GetCityPosition(string city)
    {
        var position = await _weatherService.GetCityOrDepartmentPositionAsync(city);

        if (position == null) return NotFound(); // Return NotFound if position is null

        return Ok(position); // Return Ok with the position if found
    }

    [Authorize(Policy = "CanViewData")]
    [HttpGet("search/{dateRange}/{latitude?}/{longitude?}/{category?}")]
    public async Task<IActionResult> GetWeatherData(string dateRange, string latitude, string longitude,
        string? category = null)
    {
        try
        {
            var weatherData = await _weatherService.GetWeatherData(dateRange, latitude, longitude, category);
            // compress data
            var json = JsonSerializer.Serialize(weatherData);
            var bytes = Encoding.UTF8.GetBytes(json);
            using var compressedStream = new MemoryStream();
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
            zipStream.Write(bytes, 0, bytes.Length);
            zipStream.Close();
            var compressedBytes = compressedStream.ToArray();
            return File(compressedBytes, "application/gzip");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError,
                "An error occurred while retrieving weather data.");
        }
    }
}