// Services/IJwtTokenService.cs
using WebApplication3.Models;
using System.Security.Claims;

namespace WebApplication3.Services
{
    public interface IJwtTokenService
    {
        // Genera el token JWT para un usuario
        string GenerateToken(Usuario user, IList<string> roles);
    }
}
