using System.Data;
using System.Text;
using AspNetCoreRateLimit;
using Metheo.BL;
using Metheo.DAL;
using Metheo.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper;


var builder = WebApplication.CreateBuilder(args);

// Set up Autofac container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register AuthDataAccess (which uses InvitesConnection)
builder.Services.AddScoped<IAuthDataAccess, AuthDataAccess>();

// Register named database connections using Autofac
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // Register InvitesConnection with a named qualifier
    containerBuilder.Register(c =>
    {
        var configuration = c.Resolve<IConfiguration>();
        var connectionString = "Host=localhost;Port=5432;Database=invites;Username=user;Password=password";
        return new NpgsqlConnection(connectionString);
    }).Named<IDbConnection>("InvitesConnection");

    // Register PostgresConnection with a named qualifier
    containerBuilder.Register(c =>
    {
        var configuration = c.Resolve<IConfiguration>();
        var connectionString = "Host=localhost;Port=5432;Database=laravel;Username=user;Password=password";
        return new NpgsqlConnection(connectionString);
    }).Named<IDbConnection>("PostgresConnection");
});

// Register the other dependencies (BL, etc.)
builder.Services.AddScoped<IAuthBusinessLogic, AuthService>();
builder.Services.AddScoped<IDapperWrapper, DapperWrapper>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
SqlMapper.AddTypeHandler(new ListDateTimeTypeHandler());
SqlMapper.AddTypeHandler(new ListFloatNullableTypeHandler());
SqlMapper.AddTypeHandler(new ListIntNullableTypeHandler());

// Add CORS setup before building the app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder => policyBuilder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// JWT Authentication setup
var jwtIssuer = "metheodatalatitude.com";
var jwtKey = "hm7T5BIVNhUiDbOlIPAX7RaSNJtcJ6uMm9a5OMtuVMM79";

// write in console jwtIssuer and jwtKey
Console.WriteLine(jwtIssuer);
Console.WriteLine(jwtKey);

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
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("manager", "admin"));
    options.AddPolicy("ModeratorOrManagerOrAdmin", policy => policy.RequireRole("moderateur", "manager", "admin"));
    options.AddPolicy("UserOrModeratorOrManagerOrAdmin", policy => policy.RequireRole("user", "moderateur", "manager", "admin"));
    options.AddPolicy("CanCreateData", policy => policy.RequireClaim("Permission", "create data"));
    options.AddPolicy("CanEditData", policy => policy.RequireClaim("Permission", "edit data"));
    options.AddPolicy("CanDeleteData", policy => policy.RequireClaim("Permission", "delete data"));
    options.AddPolicy("CanViewData", policy => policy.RequireClaim("Permission", "view data"));
    options.AddPolicy("CanInvite", policy => policy.RequireClaim("Permission", "can invite"));
    options.AddPolicy("CanEditProfile", policy => policy.RequireClaim("Permission", "edit profile"));
    options.AddPolicy("CanViewDashboard", policy => policy.RequireClaim("Permission", "view dashboard"));
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
            new string[] { }
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
