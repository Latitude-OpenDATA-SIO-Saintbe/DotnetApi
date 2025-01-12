using System.Net;
using Microsoft.AspNetCore.Mvc;
using Metheo.BL;

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

    // [Authorize(Policy = "Admin")]
    [HttpGet("categoriestypes")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategoryTypes()
    {
        var categoryTypes = await _weatherService.GetCategoryTypesAsync();
        return Ok(categoryTypes.Select(c => c.Name)); // Just column names
    }

    // [Authorize(Policy = "Admin")]
    [HttpGet("search/{searchCity}")]
    public async Task<ActionResult<IEnumerable<object>>> PostCities(string searchCity)
    {
        var citiesAndDepartments = await _weatherService.PostCitiesAndDepartmentsAsync(searchCity);
        return Ok(citiesAndDepartments);
    }

    // [Authorize(Policy = "Admin")]
    [HttpGet("city/{city}")]
    public async Task<ActionResult<object>> GetCityPosition(string city)
    {
        var position = await _weatherService.GetCityOrDepartmentPositionAsync(city);
        return Ok(position);
    }
    
    // [Authorize(Policy = "CanViewData")]
    [HttpGet("search/{dateRange}/{latitude?}/{longitude?}/{category?}")]
    public async Task<IActionResult> GetWeatherData(string dateRange, string latitude, string longitude, string? category = null)
    {
        try
        {
            var weatherData = await _weatherService.GetWeatherData(dateRange, latitude, longitude, category);
            return Ok(weatherData);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while retrieving weather data.");
        }
    }
}
