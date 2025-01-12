using System.Security.Claims;
using Metheo.BL;
using Metheo.DAL;
using Metheo.DTO;
using Metheo.Tools;
using Moq;

namespace Metheo.Tests.BL
{
    public class AuthServiceTests
    {
        private readonly Mock<IAuthDataAccess> _authDataAccessMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _authDataAccessMock = new Mock<IAuthDataAccess>();
            _tokenServiceMock = new Mock<ITokenService>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _authService = new AuthService(_authDataAccessMock.Object, _tokenServiceMock.Object, _passwordServiceMock.Object);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "testuser@example.com", Password = "password123" };
            var userLoginResult = new UserLoginResult { id = 1, email = "testuser@example.com", password = BCrypt.Net.BCrypt.HashPassword("password123"), role_name = "Admin", permission_name = "Read" };

            _authDataAccessMock.Setup(x => x.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync(userLoginResult);
            _passwordServiceMock.Setup(x => x.VerifyPassword(loginRequest.Password, userLoginResult.password)).Returns(true);
            _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<IEnumerable<Claim>>())).Returns("token");

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("token", result);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_ReturnsNull()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "testuser@example.com", Password = "password123" };
            var userLoginResult = new UserLoginResult { id = 1, email = "testuser@example.com", password = BCrypt.Net.BCrypt.HashPassword("differentPassword"), role_name = "Admin", permission_name = "Read" };

            _authDataAccessMock.Setup(x => x.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync(userLoginResult);
            _passwordServiceMock.Setup(x => x.VerifyPassword(loginRequest.Password, userLoginResult.password)).Returns(false);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ReturnsNull()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "nonexistentuser@example.com", Password = "password123" };

            _authDataAccessMock.Setup(x => x.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync((UserLoginResult)null!);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_EmptyEmail_ReturnsNull()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "", Password = "password123" };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_EmptyPassword_ReturnsNull()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "testuser@example.com", Password = "" };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_NullEmail_ReturnsNull()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = null!, Password = "password123" };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_NullPassword_ReturnsNull()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "testuser@example.com", Password = null! };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.Null(result);
        }
    }
}