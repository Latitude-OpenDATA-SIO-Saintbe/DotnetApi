namespace Metheo.Tests.Tools;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;
using Metheo.Tools;

public class TokenServiceTests
{
    private readonly ITokenService _tokenService;

    public TokenServiceTests()
    {
        _tokenService = new TokenService();
    }

    [Fact]
    public void GenerateToken_ValidClaims_ReturnsToken()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        // Act
        var token = _tokenService.GenerateToken(claims);

        // Assert
        Assert.NotNull(token);
        Assert.IsType<string>(token);
    }

    [Fact]
    public void GenerateToken_ValidClaims_TokenContainsClaims()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        // Act
        var token = _tokenService.GenerateToken(claims);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Name && c.Value == "testuser");
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateToken_ValidClaims_TokenHasCorrectExpiration()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        // Act
        var token = _tokenService.GenerateToken(claims);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        Assert.True(jwtToken.ValidTo <= DateTime.UtcNow.AddHours(1));
    }
}