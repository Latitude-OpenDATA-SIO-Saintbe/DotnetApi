using Metheo.DTO;
using Metheo.DAL;
using System.Threading.Tasks;
using Metheo.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Metheo.BL
{
    public interface IAuthBusinessLogic
    {
        Task<string> LoginAsync(LoginRequest request);
    }

    public class AuthBusinessLogic : IAuthBusinessLogic
    {
        private readonly IAuthDataAccess _authDataAccess;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;

        public AuthBusinessLogic(IAuthDataAccess authDataAccess, ITokenService tokenService, IPasswordService passwordService)
        {
            _authDataAccess = authDataAccess;
            _tokenService = tokenService;
            _passwordService = passwordService;
        }

        public async Task<string> LoginAsync(LoginRequest loginRequest)
        {
            // Retrieve user data
            var user = await _authDataAccess.GetUserByEmailAsync(loginRequest.Email);

            if (user == null || !_passwordService.VerifyPassword(loginRequest.Password, user.password))
            {
                return null; // Invalid credentials
            }

            // Aggregate roles and permissions
            var roles = new List<string> { user.role_name };
            var permissions = new List<string> { user.permission_name };

            // Generate JWT Token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Role, string.Join(",", roles))
            };

            claims.AddRange(permissions.Select(permission => new Claim("Permission", permission)));

            return _tokenService.GenerateToken(claims);
        }
    }
}