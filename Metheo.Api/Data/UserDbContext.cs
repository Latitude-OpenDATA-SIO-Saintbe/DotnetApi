using Microsoft.EntityFrameworkCore;
using Metheo.Api.Models;

// <summary>
/// Represents the database context for the weather application.
/// </summary>
/// <remarks>
/// This class is responsible for interacting with the database and provides DbSet properties
/// for each entity type that needs to be included in the model.
/// </remarks>
/// <param name="options">The options to be used by the DbContext.</param>
public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } // The user model with role information.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().ToTable("users"); // Specify the table name
    }
}
