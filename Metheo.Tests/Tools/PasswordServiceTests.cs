namespace Metheo.Tests.Tools;

using Xunit;
using Metheo.Tools;

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
        string inputPassword = "password123";
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(inputPassword);

        // Act
        bool result = _passwordService.VerifyPassword(inputPassword, hashedPassword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        string inputPassword = "password123";
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword("differentPassword");

        // Act
        bool result = _passwordService.VerifyPassword(inputPassword, hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_EmptyPassword_ReturnsFalse()
    {
        // Arrange
        string inputPassword = "";
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");

        // Act
        bool result = _passwordService.VerifyPassword(inputPassword, hashedPassword);

        // Assert
        Assert.False(result);
    }
}