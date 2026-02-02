using Microsoft.EntityFrameworkCore;
using Moq;
using StarshipAPI.Models;
using StarshipAPI.Services;

namespace StarshipAPI.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly StarshipDbContext _context;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<StarshipDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new StarshipDbContext(options);
            _mockJwtService = new Mock<IJwtService>();
            _authService = new AuthService(_context, _mockJwtService.Object);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ReturnsAuthResponse()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "password123"
            };

            _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("fake-jwt-token");

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("fake-jwt-token", result.Token);
            Assert.NotNull(result.User);
            Assert.Equal(request.Email, result.User.Email);
            Assert.Equal(request.Username, result.User.Username);

            // Verify user was added to database
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            Assert.NotNull(userInDb);
            Assert.True(BCrypt.Net.BCrypt.Verify(request.Password, userInDb.PasswordHash));
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingUser = new User
            {
                Email = "existing@example.com",
                Username = "existing",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var request = new RegisterRequest
            {
                Email = "existing@example.com",
                Username = "newuser",
                Password = "password123"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(request)
            );
            Assert.Equal("Email already registered.", exception.Message);
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateUsername_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingUser = new User
            {
                Email = "existing@example.com",
                Username = "existinguser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var request = new RegisterRequest
            {
                Email = "new@example.com",
                Username = "existinguser",
                Password = "password123"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(request)
            );
            Assert.Equal("Username already registered.", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var password = "password123";
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = password
            };

            _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("fake-jwt-token");

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("fake-jwt-token", result.Token);
            Assert.NotNull(result.User);
            Assert.Equal(user.Email, result.User.Email);
            Assert.Equal(user.Username, result.User.Username);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidEmail_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "password123"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(request)
            );
            Assert.Equal("Invalid email or password.", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(request)
            );
            Assert.Equal("Invalid email or password.", exception.Message);
        }

        [Fact]
        public async Task EmailExistsAsync_WithExistingEmail_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                PasswordHash = "hash"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.EmailExistsAsync("test@example.com");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EmailExistsAsync_WithNonExistingEmail_ReturnsFalse()
        {
            // Act
            var result = await _authService.EmailExistsAsync("nonexistent@example.com");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UsernameExistsAsync_WithExistingUsername_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                PasswordHash = "hash"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.UsernameExistsAsync("testuser");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UsernameExistsAsync_WithNonExistingUsername_ReturnsFalse()
        {
            // Act
            var result = await _authService.UsernameExistsAsync("nonexistent");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithExistingUser_ReturnsUserDto()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                PasswordHash = "hash"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.GetUserByIdAsync(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.Username, result.Username);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithNonExistingUser_ReturnsNull()
        {
            // Act
            var result = await _authService.GetUserByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
    }
}
