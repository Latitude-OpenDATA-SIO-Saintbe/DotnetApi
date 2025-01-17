using Metheo.Tools;
using Metheo.DTO;

namespace Metheo.Tests.Tools;

public class ApiTest
{
    [Fact]
    public void GenerateApiUrl_ReturnsCorrectUrl_WithAllParameters()
    {
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 10);
        var latitude = 48.8566f;
        var longitude = 2.3522f;
        var category = new List<CategorySearch>
        {
            new CategorySearch { Name = "temperature_2m" },
            new CategorySearch { Name = "windspeed_10m" }
        };

        var result = Api.GenerateApiUrl(startDate, endDate, latitude, longitude, category);

        Assert.Equal(
            "https://api.open-meteo.com/v1/forecast?start_date=2023-01-01&end_date=2023-01-10&latitude=48.8566&longitude=2.3522&hourly=temperature_2m,windspeed_10m",
            result);
    }

    [Fact]
    public void GenerateApiUrl_ReturnsCorrectUrl_WithoutCategory()
    {
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 10);
        var latitude = 48.8566f;
        var longitude = 2.3522f;

        var result = Api.GenerateApiUrl(startDate, endDate, latitude, longitude);

        Assert.Equal(
            "https://api.open-meteo.com/v1/forecast?start_date=2023-01-01&end_date=2023-01-10&latitude=48.8566&longitude=2.3522",
            result);
    }

    [Fact]
    public void GenerateApiUrl_ReturnsCorrectUrl_WithNegativeCoordinates()
    {
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 10);
        var latitude = -48.8566f;
        var longitude = -2.3522f;
        var category = new List<CategorySearch> { new CategorySearch { Name = "temperature_2m" } };

        var result = Api.GenerateApiUrl(startDate, endDate, latitude, longitude, category);

        Assert.Equal(
            "https://api.open-meteo.com/v1/forecast?start_date=2023-01-01&end_date=2023-01-10&latitude=-48.8566&longitude=-2.3522&hourly=temperature_2m",
            result);
    }

    [Fact]
    public void GenerateApiUrl_ReturnsCorrectUrl_WithNullCategory()
    {
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 10);
        var latitude = 48.8566f;
        var longitude = 2.3522f;
        List<CategorySearch>? category = null;

        var result = Api.GenerateApiUrl(startDate, endDate, latitude, longitude, category);

        Assert.Equal(
            "https://api.open-meteo.com/v1/forecast?start_date=2023-01-01&end_date=2023-01-10&latitude=48.8566&longitude=2.3522",
            result);
    }
}