using System.Data;
using Autofac;
using Metheo.DAL;
using Metheo.DTO;
using Moq;
using Xunit;

namespace Metheo.Tests.DAL
{
    public class AuthDataAccessTests
    {
        private readonly AuthDataAccess _authDataAccess;
        private readonly Mock<IDbConnection> _mockConnection;
        private readonly Mock<IDapperWrapper> _mockDapperWrapper;

        public AuthDataAccessTests()
        {
            _mockConnection = new Mock<IDbConnection>();
            _mockDapperWrapper = new Mock<IDapperWrapper>();

            // Create a mock or simple implementation of IComponentContext
            var mockComponentContext = new Mock<IComponentContext>();

            // Return the mock connection when ResolveNamed is called with "InvitesConnection"
            mockComponentContext
                .Setup(c => c.ResolveNamed<IDbConnection>("InvitesConnection"))
                .Returns(_mockConnection.Object);

            // Initialize the _authDataAccess with mocked dependencies
            _authDataAccess = new AuthDataAccess(mockComponentContext.Object, _mockDapperWrapper.Object);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser_WhenEmailExists()
        {
            var email = "test@example.com";
            var userLoginResult = new UserLoginResult
            {
                id = 1,
                email = email,
                password = "hashedpassword",
                role_name = "Admin",
                permission_name = "Read"
            };

            _mockDapperWrapper.Setup(d =>
                    d.QueryAsync<UserLoginResult>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new List<UserLoginResult> { userLoginResult });

            var result = await _authDataAccess.GetUserByEmailAsync(email);

            Assert.NotNull(result);
            Assert.Equal(email, result.email);
            Assert.Equal("Read", result.permission_name);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsMultiplePermissionUser_WhenEmailExists()
        {
            var email = "test@example.com";
            var userLoginResult = new UserLoginResult
            {
                id = 1,
                email = email,
                password = "hashedpassword",
                role_name = "Admin",
                permission_name = "Read, Write"
            };

            _mockDapperWrapper.Setup(d =>
                    d.QueryAsync<UserLoginResult>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new List<UserLoginResult> { userLoginResult });

            var result = await _authDataAccess.GetUserByEmailAsync(email);

            Assert.NotNull(result);
            Assert.Equal(email, result.email);
            Assert.Equal("Read, Write", result.permission_name);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsNull_WhenEmailDoesNotExist()
        {
            var email = "nonexistent@example.com";
            _mockDapperWrapper.Setup(d =>
                    d.QueryAsync<UserLoginResult>(_mockConnection.Object, It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(Enumerable.Empty<UserLoginResult>());

            var result = await _authDataAccess.GetUserByEmailAsync(email);

            Assert.Null(result);
        }
    }
}
