using Microsoft.EntityFrameworkCore;
using Metheo.Api; // Metheo is the correct namespace
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Set up DbContext with PostgreSQL
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"))); // Npgsql is the correct namespace

// Configure User DbContext for invites database
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("InvitesConnection"))); // Npgsql is the correct namespace

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

var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();

// Add authentication and JWT bearer token support
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

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully.");
            return Task.CompletedTask;
        }
    };
});


// Add authorization policies based on role or permission
// Add authorization policies for roles and permissions
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

// Register other services as needed (e.g., Swagger, logging, etc.)
builder.Services.AddLogging();

var app = builder.Build();

// Configure the middleware pipeline
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication(); // Enables authentication middleware
app.UseAuthorization(); // Enables authorization middleware

app.MapControllers();

app.Run();
