// Data/WeatherDbContext.cs
using Microsoft.EntityFrameworkCore;
using Metheo.Api.Models;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options) { }

    public DbSet<CategoryType> CategoryTypes { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Departement> Departements { get; set; }
    public DbSet<WeatherData> WeatherDatas { get; set; }
    public DbSet<WeatherStation> WeatherStation { get; set; }
}