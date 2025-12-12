// Controllers/PrestamosController.cs
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
    public class PrestamosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PrestamosController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // ------------------------------------------------------------------
        // 1. READ ALL: Obtener todos los préstamos
        // ------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrestamoResponseDto>>> GetPrestamos()
        {
            var userId = GetUserId();
            var esAdmin = User.IsInRole(SeedData.AdminRole);

            var query = _context.Prestamos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Libro)
                .AsQueryable();

            // Lógica de Autorización: Si no es Admin, solo ve sus préstamos
            if (!esAdmin)
            {
                query = query.Where(p => p.UsuarioId == userId);
            }

            var prestamos = await query
                .Select(p => new PrestamoResponseDto
                {
                    Id = p.Id,
                    FechaPrestamo = p.FechaPrestamo,
                    FechaDevolucionPrevista = p.FechaDevolucionPrevista,
                    FechaDevolucionReal = p.FechaDevolucionReal,
                    UsuarioId = p.UsuarioId,
                    UsuarioUsername = p.Usuario.UserName,
                    Detalles = p.Detalles.Select(d => new DetallePrestamoResponseDto
                    {
                        Id = d.Id,
                        LibroId = d.LibroId,
                        LibroTitulo = d.Libro.Titulo,
                        Cantidad = d.Cantidad,
                        MultaId = d.Multa.Id
                    }).ToList()
                })
                .ToListAsync();

            return Ok(prestamos);
        }

        // ------------------------------------------------------------------
        // 2. CREATE: Crear un nuevo préstamo (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<ActionResult<PrestamoResponseDto>> PostPrestamo([FromBody] PrestamoCreateDto prestamoDto)
        {
            // Validación básica de la fecha
            if (prestamoDto.FechaDevolucionPrevista <= DateTime.UtcNow)
            {
                return BadRequest("La fecha de devolución prevista debe ser en el futuro.");
            }

            var nuevoPrestamo = new Prestamo
            {
                UsuarioId = GetUserId(), // El Administrador realiza el préstamo para sí mismo (simplificación)
                                         // En un sistema real, el DTO debería llevar el UsuarioId del prestatario.
                FechaPrestamo = DateTime.UtcNow,
                FechaDevolucionPrevista = prestamoDto.FechaDevolucionPrevista,
                Detalles = new List<DetallePrestamo>()
            };

            // 1. Procesar Detalles y validar libros y stock
            foreach (var detalleDto in prestamoDto.Detalles)
            {
                var libro = await _context.Libros.FindAsync(detalleDto.LibroId);

                if (libro == null)
                {
                    return BadRequest($"Libro con ID {detalleDto.LibroId} no encontrado.");
                }
                if (libro.CantidadDisponible < detalleDto.Cantidad)
                {
                    return BadRequest($"Stock insuficiente para el libro {libro.Titulo}. Disponible: {libro.CantidadDisponible}.");
                }

                // Actualizar stock
                libro.CantidadDisponible -= detalleDto.Cantidad;

                // Crear Detalle
                nuevoPrestamo.Detalles.Add(new DetallePrestamo
                {
                    LibroId = detalleDto.LibroId,
                    Cantidad = detalleDto.Cantidad
                });
            }

            _context.Prestamos.Add(nuevoPrestamo);
            await _context.SaveChangesAsync();

            // Retornar el DTO de respuesta (requiere recargar o mapear cuidadosamente)
            var response = await _context.Prestamos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Libro)
                .Where(p => p.Id == nuevoPrestamo.Id)
                .Select(p => new PrestamoResponseDto
                {
                    Id = p.Id,
                    FechaPrestamo = p.FechaPrestamo,
                    FechaDevolucionPrevista = p.FechaDevolucionPrevista,
                    FechaDevolucionReal = p.FechaDevolucionReal,
                    UsuarioId = p.UsuarioId,
                    UsuarioUsername = p.Usuario.UserName,
                    Detalles = p.Detalles.Select(d => new DetallePrestamoResponseDto
                    {
                        Id = d.Id,
                        LibroId = d.LibroId,
                        LibroTitulo = d.Libro.Titulo,
                        Cantidad = d.Cantidad,
                        MultaId = d.Multa.Id
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetPrestamos), new { id = nuevoPrestamo.Id }, response);
        }

        // ------------------------------------------------------------------
        // 3. UPDATE (Devolución): Marcar un préstamo/detalle como devuelto (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpPut("{id}/devolver")]
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<IActionResult> DevolverPrestamo(int id)
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prestamo == null || prestamo.FechaDevolucionReal.HasValue)
            {
                return prestamo == null ? NotFound() : BadRequest("Este préstamo ya fue devuelto.");
            }

            // 1. Marcar el Préstamo como devuelto
            prestamo.FechaDevolucionReal = DateTime.UtcNow;

            // 2. Procesar Detalles: Actualizar stock y verificar multas
            var fechaActual = DateTime.UtcNow;
            var librosAfectados = new List<Libro>();

            foreach (var detalle in prestamo.Detalles)
            {
                var libro = await _context.Libros.FindAsync(detalle.LibroId);

                // 2a. Aumentar stock
                if (libro != null)
                {
                    libro.CantidadDisponible += detalle.Cantidad;
                    librosAfectados.Add(libro);
                }

                // 2b. Verificar Multa
                if (fechaActual > prestamo.FechaDevolucionPrevista)
                {
                    // Lógica simplificada: Generar multa por cada detalle devuelto tarde
                    var diasRetraso = (int)(fechaActual - prestamo.FechaDevolucionPrevista).TotalDays;

                    var multa = new Multa
                    {
                        UsuarioId = prestamo.UsuarioId,
                        DetallePrestamoId = detalle.Id,
                        Monto = diasRetraso * 1.0m, // 1.0 unidad monetaria por día (ejemplo)
                        FechaGeneracion = fechaActual,
                        Estado = "Pendiente"
                    };
                    _context.Multas.Add(multa);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Préstamo devuelto y multas generadas (si aplica)." });
        }

        // ------------------------------------------------------------------
        // 4. DELETE: Eliminar un préstamo (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpDelete("{id}")]
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<IActionResult> DeletePrestamo(int id)
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prestamo == null)
            {
                return NotFound();
            }

            // Nota: Para fines del CRUD, eliminamos detalles y la cabecera.
            // En un sistema real, no se debería eliminar un préstamo completado.
            _context.DetallesPrestamo.RemoveRange(prestamo.Detalles);
            _context.Prestamos.Remove(prestamo);

            // Revertir el stock si el préstamo no había sido devuelto
            if (!prestamo.FechaDevolucionReal.HasValue)
            {
                foreach (var detalle in prestamo.Detalles)
                {
                    var libro = await _context.Libros.FindAsync(detalle.LibroId);
                    if (libro != null)
                    {
                        libro.CantidadDisponible += detalle.Cantidad;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}