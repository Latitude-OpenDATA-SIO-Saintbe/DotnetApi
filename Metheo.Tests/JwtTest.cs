using Moq;
using Npgsql;
using Metheo.Api.Controllers;
using Metheo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Metheo.Tests
{
    /// <summary>
    /// Unit tests for the Jwt functionality in the AuthController.
    /// </summary>
    public class JwtTest
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly Mock<NpgsqlConnection> _mockConnection;
        private readonly Mock<NpgsqlCommand> _mockCommand;
        private readonly Mock<NpgsqlDataReader> _mockReader;
        private readonly AuthController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTest"/> class.
        /// </summary>
        public JwtTest()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _mockConnection = new Mock<NpgsqlConnection>("Host=localhost;Port=5432;Database=invites;Username=user;Password=password");
            _mockCommand = new Mock<NpgsqlCommand>();
            _mockReader = new Mock<NpgsqlDataReader>();
            _controller = new AuthController(_mockConfig.Object, _mockLogger.Object);
        }

        /// <summary>
        /// Tests the Login method of the AuthController.
        /// Mocks the behavior of NpgsqlCommand and NpgsqlDataReader to simulate a database call.kCo
        /// Verifies that the Login method returns an OkObjectResult with a valid token and expiration when provided with valid credentials.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [Fact]
        public async Task Login_ReturnsOkResult_WhenValidCredentials()
        {
            // Arrange: Mock the behavior of NpgsqlCommand and NpgsqlDataReader
            var loginRequest = new LoginRequest { Email = "user@example.com", Password = "validPassword" };

            // Mock Dapper's QueryAsync method: Setup NpgsqlCommand and ExecuteReaderAsync
            var mockUserLoginResult = new List<UserLoginResult>
            {
                new UserLoginResult
                {
                    id = 1,
                    email = "user@example.com",
                    password = BCrypt.Net.BCrypt.HashPassword("validPassword"), // Correct password hash in DB
                    role_name = "Admin",
                    permission_name = "Read"
                }
            };

            // Mock the NpgsqlCommand to simulate calling ExecuteReaderAsync
            _mockCommand.Setup(c => c.ExecuteReaderAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockReader.Object);

            // Mock the CreateCommand method to return the mock command
            _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);

            // Mock the NpgsqlDataReader to simulate reading data from the database
            _mockReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true) // Simulate the reader having data
                .ReturnsAsync(false); // Simulate no more data after the first row

            _mockReader.Setup(r => r.GetString(It.IsAny<int>())).Returns("Temperature"); // Simulate reading a column value

            // Mock the configuration to return a connection string
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x.GetConnectionString("InvitesConnection")).Returns("Host=localhost;Port=5432;Database=invites;Username=user;Password=password");

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert: Ensure the result is as expected
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<dynamic>(okResult.Value);
            Assert.NotNull(value.Token);
            Assert.NotNull(value.Expiration);
        }
    }
}
