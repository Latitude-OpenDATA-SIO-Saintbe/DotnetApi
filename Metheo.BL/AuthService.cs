using System.Security.Claims;
using Metheo.DAL;
using Metheo.DTO;
using Metheo.Tools;

namespace Metheo.BL;

public interface IAuthBusinessLogic
{
    Task<string> LoginAsync(LoginRequest? request);
}

public class AuthService : IAuthBusinessLogic
{
    private readonly IAuthDataAccess _authDataAccess;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public AuthService(IAuthDataAccess authDataAccess, ITokenService tokenService, IPasswordService passwordService)
    {
        _authDataAccess = authDataAccess;
        _tokenService = tokenService;
        _passwordService = passwordService;
    }

    public async Task<string> LoginAsync(LoginRequest? loginRequest)
    {
        // Retrieve user data
        var user = await _authDataAccess.GetUserByEmailAsync(loginRequest.Email);

        if (user == null ||
            !_passwordService.VerifyPassword(loginRequest.Password,
                user.password)) return null!; // Invalid credentials or user not found

        // Aggregate roles and permissions
        var roles = new List<string> { user.role_name };
        var permissions = new List<string> { user.permission_name };

        // Generate JWT Token
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.id.ToString()),
            new(ClaimTypes.Email, user.email),
            new(ClaimTypes.Role, string.Join(",", roles))
        };

        var permissionList = permissions.First().Split(',').Select(p => p.Trim()).ToList();
        claims.AddRange(permissionList.Select(permission => new Claim("Permission", permission)));

        return _tokenService.GenerateToken(claims);
    }
}