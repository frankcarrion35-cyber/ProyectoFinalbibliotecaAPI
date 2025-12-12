// Controllers/AuthController.cs (VERSIÓN FINAL)
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.DTOs;
using WebApplication3.Models;
using WebApplication3.Services;
using WebApplication3.Data;
using Microsoft.Extensions.Configuration; // Necesario para inyectar IConfiguration

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IJwtTokenService _tokenService;
        private readonly IConfiguration _configuration; // Inyección de configuración

        public AuthController(UserManager<Usuario> userManager, IJwtTokenService tokenService, IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status400BadRequest, new { Status = "Error", Message = "El usuario ya existe." });

            Usuario user = new Usuario()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                NombreCompleto = model.NombreCompleto
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "Fallo la creación del usuario." });

            await _userManager.AddToRoleAsync(user, SeedData.LectorRole);

            return Ok(new { Status = "Success", Message = "Usuario creado exitosamente. Rol asignado: Lector" });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var token = _tokenService.GenerateToken(user, userRoles);

                // Obtener la duración del JWT de la configuración inyectada
                var durationMinutes = double.Parse(_configuration["Jwt:DurationInMinutes"]);

                return Ok(new
                {
                    token = token,
                    username = user.UserName,
                    roles = userRoles,
                    expiration = DateTime.Now.AddMinutes(durationMinutes)
                });
            }
            return Unauthorized(new { Status = "Error", Message = "Usuario o contraseña inválidos." });
        }
    }
}