// Data/WeatherDbContext.cs
using Microsoft.EntityFrameworkCore;
using Metheo.Api.Models;

/// <summary>
/// Represents the database context for the weather application.
/// </summary>
/// <remarks>
/// This class is responsible for interacting with the database and provides DbSet properties
/// for each entity type that needs to be included in the model.
/// </remarks>
/// <param name="options">The options to be used by the DbContext.</param>
public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options) { }

    public DbSet<CategoryType> CategoryTypes { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Departement> Departements { get; set; }
    public DbSet<WeatherData> WeatherDatas { get; set; }
    public DbSet<WeatherStation> WeatherStation { get; set; }
}
