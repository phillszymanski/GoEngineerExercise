using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarshipAPI.Models;
using StarshipAPI.Services;

namespace StarshipAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService,IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterRequest request)
        {
            if(string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "All fields are required." });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "Password must be at least 6 characters" });
            }

            try
            {
                var response = await _authService.RegisterAsync(request);
                SetAuthCookie(response.Token);
                return Ok(response.User);
            } 
            catch(InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                SetAuthCookie(response.Token);
                return Ok(response.User);
            }
            catch(UnauthorizedAccessException ex)
            {
                return Unauthorized(new {message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            var userId = int.Parse(userIdClaim);
            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("authToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });
            return Ok(new { message = "Logged out successfully" });
        }

        private void SetAuthCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict, 
                Expires = DateTimeOffset.UtcNow.AddHours(24),
                Path = "/"
            };
            Response.Cookies.Append("authToken", token, cookieOptions);
        }
    }
}