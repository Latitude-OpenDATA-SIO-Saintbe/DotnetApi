using Metheo.Tools;

namespace Metheo.Tests.Tools;

public class PasswordServiceTests
{
    private readonly IPasswordService _passwordService;

    public PasswordServiceTests()
    {
        _passwordService = new PasswordService();
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var inputPassword = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(inputPassword);

        // Act
        var result = _passwordService.VerifyPassword(inputPassword, hashedPassword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var inputPassword = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("differentPassword");

        // Act
        var result = _passwordService.VerifyPassword(inputPassword, hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_EmptyPassword_ReturnsFalse()
    {
        // Arrange
        var inputPassword = "";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");

        // Act
        var result = _passwordService.VerifyPassword(inputPassword, hashedPassword);

        // Assert
        Assert.False(result);
    }
}