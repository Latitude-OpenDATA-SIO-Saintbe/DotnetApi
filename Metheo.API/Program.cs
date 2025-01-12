using System.Data;
using Npgsql;
using Microsoft.Extensions.DependencyInjection;
using Metheo.DAL;
using Metheo.BL;
using Metheo.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Extensions.Configuration;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// Add controller support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register PostgreSQL connection for the authentication database (IDbConnection for Auth)
builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("InvitesConnection")));

// Register PostgreSQL connection for the data access database (IDbConnection for Data)
builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("PostgresConnection")));

// Register the dependencies (BL, DAL, etc.)
builder.Services.AddScoped<IAuthDataAccess, AuthDataAccess>();
builder.Services.AddScoped<IAuthBusinessLogic, AuthBusinessLogic>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
builder.Services.AddScoped<IWeatherService, WeatherService>();

// Add CORS setup before building the app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder => policyBuilder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// JWT Authentication setup
var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.UseSecurityTokenValidators = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = false,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddMemoryCache();


// Authorization policies setup
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("manager", "admin"));
    options.AddPolicy("ModeratorOrAdmin", policy => policy.RequireRole("moderateur", "admin"));
    options.AddPolicy("UserOrModeratorOrManagerOrAdmin", policy => policy.RequireRole("user", "moderateur", "manager", "admin"));

    // Permission-based policies
    options.AddPolicy("CanCreateData", policy => policy.RequireClaim("Permission", "create data"));
    options.AddPolicy("CanEditData", policy => policy.RequireClaim("Permission", "edit data"));
    options.AddPolicy("CanDeleteData", policy => policy.RequireClaim("Permission", "delete data"));
    options.AddPolicy("CanViewData", policy => policy.RequireClaim("Permission", "view data"));
    options.AddPolicy("CanInvite", policy => policy.RequireClaim("Permission", "can invite"));
    options.AddPolicy("CanEditProfile", policy => policy.RequireClaim("Permission", "edit profile"));
    options.AddPolicy("CanViewDashboard", policy => policy.RequireClaim("Permission", "view dashboard"));

    // Combined role or permission-based policies
    options.AddPolicy("AdminOrCreateData", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("admin") || context.User.HasClaim(c => c.Type == "Permission" && c.Value == "create data")
        ));
    options.AddPolicy("ManagerOrEditData", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("manager") || context.User.HasClaim(c => c.Type == "Permission" && c.Value == "edit data")
        ));
});

// Add AspNetCoreRateLimit services
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Configure Swagger authentication
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddControllers();

// Configure the HTTP request pipeline
var app = builder.Build();

// Enable Swagger UI if in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapControllers();
app.Run();
