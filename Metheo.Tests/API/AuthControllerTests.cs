using ApiDotnetMetheoOrm.Controller.Auth;
using Metheo.BL;
using Metheo.DTO;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Metheo.Tests.API;

public class AuthControllerTests
{
    private readonly Mock<IAuthBusinessLogic> _authBusinessLogicMock;
    private readonly AuthController _authController;

    public AuthControllerTests()
    {
        _authBusinessLogicMock = new Mock<IAuthBusinessLogic>();
        _authController = new AuthController(_authBusinessLogicMock.Object);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkResultWithToken()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "testuser", Password = "password" };
        var expectedToken = "mocked-jwt-token";
        _authBusinessLogicMock.Setup(x => x.LoginAsync(loginRequest)).ReturnsAsync(expectedToken);

        // Act
        var result = await _authController.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = okResult.Value;
        Assert.NotNull(returnValue);
        Assert.Equal(expectedToken, returnValue.GetType().GetProperty("Token").GetValue(returnValue, null));
    }

    [Fact]
    public async Task Login_NullRequest_ReturnsBadRequest()
    {
        // Act
        var result = await _authController.Login(null);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }
}