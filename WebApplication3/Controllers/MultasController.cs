// Controllers/MultasController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.DTOs;
using WebApplication3.Models;
using System.Security.Claims;

namespace WebApplication3.Controllers
{
    // Requerir autenticación JWT para todos los endpoints
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MultasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MultasController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // ------------------------------------------------------------------
        // 1. READ ALL: Obtener todas las multas (Admin puede ver todas, Lector solo las suyas)
        // ------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MultaResponseDto>>> GetMultas()
        {
            var userId = GetUserId();
            var esAdmin = User.IsInRole(SeedData.AdminRole);

            var query = _context.Multas
                .Include(m => m.Usuario)
                .AsQueryable();

            // Lógica de Autorización: Si no es Admin, solo ve sus multas
            if (!esAdmin)
            {
                query = query.Where(m => m.UsuarioId == userId);
            }

            var multas = await query
                .Select(m => new MultaResponseDto
                {
                    Id = m.Id,
                    Monto = m.Monto,
                    FechaGeneracion = m.FechaGeneracion,
                    FechaPago = m.FechaPago,
                    Estado = m.Estado,
                    UsuarioId = m.UsuarioId,
                    UsuarioUsername = m.Usuario.UserName,
                    DetallePrestamoId = m.DetallePrestamoId
                })
                .ToListAsync();

            return Ok(multas);
        }

        // ------------------------------------------------------------------
        // 2. READ SINGLE: Obtener multa por ID (Requiere ser Admin o dueño de la multa)
        // ------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<MultaResponseDto>> GetMulta(int id)
        {
            var multa = await _context.Multas
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (multa == null)
            {
                return NotFound();
            }

            // Autorización: Solo Admin o el dueño de la multa puede verla
            var userId = GetUserId();
            if (multa.UsuarioId != userId && !User.IsInRole(SeedData.AdminRole))
            {
                return Forbid(); // 403 Forbidden
            }

            return Ok(new MultaResponseDto
            {
                Id = multa.Id,
                Monto = multa.Monto,
                FechaGeneracion = multa.FechaGeneracion,
                FechaPago = multa.FechaPago,
                Estado = multa.Estado,
                UsuarioId = multa.UsuarioId,
                UsuarioUsername = multa.Usuario.UserName,
                DetallePrestamoId = multa.DetallePrestamoId
            });
        }


        // ------------------------------------------------------------------
        // 3. UPDATE (Estado): Marcar multa como pagada/anulada (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpPut("{id}/estado")]
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<IActionResult> PutMultaEstado(int id, [FromBody] MultaUpdateEstadoDto estadoDto)
        {
            var multa = await _context.Multas.FindAsync(id);
            if (multa == null)
            {
                return NotFound();
            }

            // Lógica de negocio: Si se marca como "Pagada", registrar la fecha de pago
            if (estadoDto.Estado.Equals("Pagada", StringComparison.OrdinalIgnoreCase) && !multa.FechaPago.HasValue)
            {
                multa.FechaPago = DateTime.UtcNow;
            }

            multa.Estado = estadoDto.Estado;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Manejo de concurrencia
                return BadRequest("Error de concurrencia al actualizar la multa.");
            }

            return NoContent(); // 204 Success
        }

        // ------------------------------------------------------------------
        // 4. DELETE: Eliminar una multa (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpDelete("{id}")]
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<IActionResult> DeleteMulta(int id)
        {
            var multa = await _context.Multas.FindAsync(id);
            if (multa == null)
            {
                return NotFound();
            }

            // La eliminación de multas generalmente solo la hace el administrador
            _context.Multas.Remove(multa);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}