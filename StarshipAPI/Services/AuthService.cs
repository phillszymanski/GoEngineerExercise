using Microsoft.EntityFrameworkCore;
using StarshipAPI.Models;

namespace StarshipAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly StarshipDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(StarshipDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user == null ? null : MapToUserDto(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                                .FirstOrDefaultAsync(u => u.Email == request.Email);
            
            if(user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var token = _jwtService.GenerateToken(user);

            return new AuthResponse
            {
                Token = token,
                User = MapToUserDto(user)
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if(await EmailExistsAsync(request.Email))
            {
                throw new InvalidOperationException("Email already registered.");
            }

            if(await UsernameExistsAsync(request.Username))
            {
                throw new InvalidOperationException("Username already registered.");
            }

            var user = new User
            {
                Email = request.Email,
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            return new AuthResponse
            {
                Token = token,
                User = MapToUserDto(user)
            };
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username
            };
        }
    }
}