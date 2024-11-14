using Microsoft.EntityFrameworkCore;
using Metheo.Api;

var builder = WebApplication.CreateBuilder(args);

// Set up DbContext with PostgreSQL
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder => policyBuilder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var app = builder.Build();

// Configure the middleware pipeline
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

// app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
