using System.Security.Claims;
using StarshipAPI.Models;

namespace StarshipAPI.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}