using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Npgsql;
using BCrypt.Net;
using Microsoft.Extensions.Logging; // Added logging
using System.Threading.Tasks;
using Metheo.Api.Models;
using Dapper;

namespace Metheo.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        // Constructor injection for configuration and logger
        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
            {
                return BadRequest("Invalid login request.");
            }

            // Connection string to PostgreSQL database
            var connectionString = _configuration.GetConnectionString("InvitesConnection");

            // Create a new PostgreSQL connection
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // SQL query to get user, roles, and permissions
                var query = @"
                    SELECT u.id, u.email, u.password, r.name AS role_name, p.name AS permission_name
                    FROM users u
                    LEFT JOIN model_has_roles ur ON u.id = ur.model_id
                    LEFT JOIN roles r ON ur.role_id = r.id
                    LEFT JOIN role_has_permissions rp ON r.id = rp.role_id
                    LEFT JOIN permissions p ON rp.permission_id = p.id
                    WHERE u.email = @Email";

                // Execute the query using Dapper and parameterize the email
                var result = await connection.QueryAsync<UserLoginResult>(query, new { Email = login.Email });

                var user = result.FirstOrDefault();

                // verify password of user
                if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.password))
                {
                    return Unauthorized("Invalid credentials.");
                }

                // Aggregate roles and permissions
                var userRoles = result
                    .Where(r => r.role_name != null)
                    .Select(r => r.role_name)
                    .Distinct()
                    .ToList();

                var userPermissions = result
                    .Where(r => r.permission_name != null)
                    .Select(r => r.permission_name)
                    .Distinct()
                    .ToList();

                // Define claims - Include roles and permissions as claims
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                // Include roles as claims
                claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

                // Include permissions as claims
                claims.AddRange(userPermissions.Select(permission => new Claim("Permission", permission)));

                var claimsArray = claims.ToArray();

                // JWT token creation code as before
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddYears(1),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Issuer"],
                    SigningCredentials = creds
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwt = tokenHandler.WriteToken(token);

                // Log the login activity
                _logger.LogInformation($"User {user.email}/{string.Join(",", userRoles)}/{string.Join(",", userPermissions)} logged in successfully.");

                return Ok(new
                {
                    Token = jwt,
                    Expiration = tokenDescriptor.Expires,
                });
            }
        }
    }
}
