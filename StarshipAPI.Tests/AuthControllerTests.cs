using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using StarshipAPI.Controllers;
using StarshipAPI.Models;
using StarshipAPI.Services;
using Xunit;

namespace StarshipAPI.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _controller = new AuthController(_mockAuthService.Object, _mockConfiguration.Object);
            
            // Setup HttpContext and Response for cookie operations
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenRegistrationSuccessful()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "password123"
            };

            var userDto = new UserDto
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser"
            };

            var authResponse = new AuthResponse
            {
                Token = "fake-jwt-token",
                User = userDto
            };

            _mockAuthService
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal("test@example.com", returnedUser.Email);
            Assert.Equal("testuser", returnedUser.Username);
            
            _mockAuthService.Verify(s => s.RegisterAsync(It.Is<RegisterRequest>(
                r => r.Email == "test@example.com" && 
                     r.Username == "testuser" && 
                     r.Password == "password123"
            )), Times.Once);
        }

        [Fact]
        public async Task Register_ShouldSetAuthCookie_WhenSuccessful()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "password123"
            };

            var authResponse = new AuthResponse
            {
                Token = "test-token-12345",
                User = new UserDto { Id = 1, Email = "test@example.com", Username = "testuser" }
            };

            _mockAuthService
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ReturnsAsync(authResponse);

            // Act
            await _controller.Register(request);

            // Assert
            var cookies = _controller.Response.Headers["Set-Cookie"].ToString();
            Assert.Contains("authToken=", cookies);
            Assert.Contains("test-token-12345", cookies);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailIsEmpty()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "",
                Username = "testuser",
                Password = "password123"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = badRequestResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);
            Assert.Equal("All fields are required.", message);
            
            _mockAuthService.Verify(s => s.RegisterAsync(It.IsAny<RegisterRequest>()), Times.Never);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenUsernameIsEmpty()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Username = "",
                Password = "password123"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = badRequestResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);
            Assert.Equal("All fields are required.", message);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenPasswordIsEmpty()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = ""
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = badRequestResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);
            Assert.Equal("All fields are required.", message);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenPasswordTooShort()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "12345" // Only 5 characters
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = badRequestResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);
            Assert.Equal("Password must be at least 6 characters", message);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "existing@example.com",
                Username = "testuser",
                Password = "password123"
            };

            _mockAuthService
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ThrowsAsync(new InvalidOperationException("Email already exists"));

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = badRequestResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);
            Assert.Equal("Email already exists", message);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenCredentialsValid()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var userDto = new UserDto
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser"
            };

            var authResponse = new AuthResponse
            {
                Token = "fake-jwt-token",
                User = userDto
            };

            _mockAuthService
                .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal("test@example.com", returnedUser.Email);
            Assert.Equal("testuser", returnedUser.Username);
            
            _mockAuthService.Verify(s => s.LoginAsync(It.Is<LoginRequest>(
                r => r.Email == "test@example.com" && r.Password == "password123"
            )), Times.Once);
        }

        [Fact]
        public async Task Login_ShouldSetAuthCookie_WhenSuccessful()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var authResponse = new AuthResponse
            {
                Token = "login-token-67890",
                User = new UserDto { Id = 1, Email = "test@example.com", Username = "testuser" }
            };

            _mockAuthService
                .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(authResponse);

            // Act
            await _controller.Login(request);

            // Assert
            var cookies = _controller.Response.Headers["Set-Cookie"].ToString();
            Assert.Contains("authToken=", cookies);
            Assert.Contains("login-token-67890", cookies);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsInvalid()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            _mockAuthService
                .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = unauthorizedResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);
            Assert.Equal("Invalid credentials", message);
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnOk_WhenUserAuthenticated()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser"
            };

            _mockAuthService
                .Setup(s => s.GetUserByIdAsync(1))
                .ReturnsAsync(userDto);

            // Setup authenticated user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext.User = claimsPrincipal;

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(1, returnedUser.Id);
            Assert.Equal("test@example.com", returnedUser.Email);
            
            _mockAuthService.Verify(s => s.GetUserByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenNotAuthenticated()
        {
            // Arrange - No claims set, simulating unauthenticated user
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = unauthorizedResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);
            Assert.Equal("Not authenticated", message);
            
            _mockAuthService.Verify(s => s.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _mockAuthService
                .Setup(s => s.GetUserByIdAsync(999))
                .ReturnsAsync((UserDto?)null);

            // Setup authenticated user claims with non-existent user ID
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "999")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext.User = claimsPrincipal;

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
            _mockAuthService.Verify(s => s.GetUserByIdAsync(999), Times.Once);
        }

        [Fact]
        public void Logout_ShouldReturnOk()
        {
            // Act
            var result = _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);
            Assert.Equal("Logged out successfully", message);
        }

        [Fact]
        public void Logout_ShouldDeleteAuthCookie()
        {
            // Arrange
            // First set a cookie
            _controller.Response.Cookies.Append("authToken", "some-token");

            // Act
            _controller.Logout();

            // Assert
            // The cookie should be deleted (expires set to past date)
            var cookies = _controller.Response.Headers["Set-Cookie"].ToString();
            Assert.Contains("authToken=", cookies);
            Assert.Contains("expires=", cookies.ToLower());
        }
    }
}
