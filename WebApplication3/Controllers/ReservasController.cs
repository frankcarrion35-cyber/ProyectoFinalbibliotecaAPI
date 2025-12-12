// Controllers/ReservasController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.DTOs;
using WebApplication3.Models;
using System.Security.Claims; // Para acceder a los datos del usuario logueado

namespace WebApplication3.Controllers
{
    // Requerir autenticación JWT para todos los endpoints
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReservasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Función de ayuda para obtener el ID del usuario logueado
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // ------------------------------------------------------------------
        // 1. READ ALL: Obtener todas las reservas (Admin puede ver todas, Lector solo las suyas)
        // ------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservaResponseDto>>> GetReservas()
        {
            var userId = GetUserId();
            var esAdmin = User.IsInRole(SeedData.AdminRole);

            var query = _context.Reservas
                .Include(r => r.Libro)
                .Include(r => r.Usuario)
                .AsQueryable();

            // Lógica de Autorización: Si no es Admin, solo ve sus reservas
            if (!esAdmin)
            {
                query = query.Where(r => r.UsuarioId == userId);
            }

            var reservas = await query
                .Select(r => new ReservaResponseDto
                {
                    Id = r.Id,
                    FechaReserva = r.FechaReserva,
                    FechaExpiracion = r.FechaExpiracion,
                    Estado = r.Estado,
                    LibroId = r.LibroId,
                    LibroTitulo = r.Libro.Titulo,
                    UsuarioId = r.UsuarioId,
                    UsuarioUsername = r.Usuario.UserName
                })
                .ToListAsync();

            return Ok(reservas);
        }

        // ------------------------------------------------------------------
        // 2. READ SINGLE: Obtener reserva por ID (Requiere ser Admin o dueño de la reserva)
        // ------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservaResponseDto>> GetReserva(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Libro)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            // Autorización: Solo Admin o el dueño de la reserva puede verla
            var userId = GetUserId();
            if (reserva.UsuarioId != userId && !User.IsInRole(SeedData.AdminRole))
            {
                return Forbid(); // 403 Forbidden
            }

            return Ok(new ReservaResponseDto
            {
                Id = reserva.Id,
                FechaReserva = reserva.FechaReserva,
                FechaExpiracion = reserva.FechaExpiracion,
                Estado = reserva.Estado,
                LibroId = reserva.LibroId,
                LibroTitulo = reserva.Libro.Titulo,
                UsuarioId = reserva.UsuarioId,
                UsuarioUsername = reserva.Usuario.UserName
            });
        }


        // ------------------------------------------------------------------
        // 3. CREATE: Crear una nueva reserva (Accesible por Lector y Admin)
        // ------------------------------------------------------------------
        [HttpPost]
        public async Task<ActionResult<ReservaResponseDto>> PostReserva([FromBody] ReservaCreateDto reservaDto)
        {
            var libro = await _context.Libros.FindAsync(reservaDto.LibroId);
            if (libro == null)
            {
                return BadRequest("Libro no encontrado.");
            }

            // Lógica de negocio básica: Verificar si hay ejemplares disponibles para reservar
            if (libro.CantidadDisponible <= 0)
            {
                // Nota: En un sistema real, se debería verificar si el libro está reservado pero no prestado.
                return BadRequest("El libro no tiene ejemplares disponibles para reserva.");
            }

            var userId = GetUserId();

            var reserva = new Reserva
            {
                LibroId = reservaDto.LibroId,
                UsuarioId = userId,
                FechaReserva = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddDays(3), // Expira en 3 días
                Estado = "Pendiente"
            };

            _context.Reservas.Add(reserva);

            // Opcional: Reducir la cantidad disponible si la reserva bloquea un ejemplar
            // libro.CantidadDisponible--; 

            await _context.SaveChangesAsync();

            // Mapeo a Response DTO (requiere cargar las relaciones para la respuesta)
            await _context.Entry(reserva).Reference(r => r.Usuario).LoadAsync();

            var responseDto = new ReservaResponseDto
            {
                Id = reserva.Id,
                FechaReserva = reserva.FechaReserva,
                FechaExpiracion = reserva.FechaExpiracion,
                Estado = reserva.Estado,
                LibroId = libro.Id,
                LibroTitulo = libro.Titulo,
                UsuarioId = reserva.UsuarioId,
                UsuarioUsername = reserva.Usuario.UserName
            };

            return CreatedAtAction(nameof(GetReserva), new { id = reserva.Id }, responseDto);
        }

        // ------------------------------------------------------------------
        // 4. UPDATE (Estado): Actualizar el estado de una reserva (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpPut("{id}/estado")]
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<IActionResult> PutReservaEstado(int id, [FromBody] ReservaUpdateEstadoDto estadoDto)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }

            reserva.Estado = estadoDto.Estado;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Manejo de concurrencia
                return BadRequest("Error de concurrencia al actualizar la reserva.");
            }

            return NoContent(); // 204 Success
        }

        // ------------------------------------------------------------------
        // 5. DELETE: Eliminar una reserva (Sólo Administrador o dueño que cancela)
        // ------------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }

            // Autorización: Solo Admin o el dueño de la reserva puede eliminar/cancelar
            var userId = GetUserId();
            if (reserva.UsuarioId != userId && !User.IsInRole(SeedData.AdminRole))
            {
                return Forbid(); // 403 Forbidden
            }

            // En lugar de borrar, la lógica real sería cambiar el estado a "Cancelada"
            // Para fines del CRUD básico requerido, se permite la eliminación directa:
            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}