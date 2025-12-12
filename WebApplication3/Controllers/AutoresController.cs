using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.DTOs;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    // Todos los endpoints requerirán un token JWT válido
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AutoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------
        // 1. READ ALL: Obtener todos los autores (Accesible por Lector y Admin)
        // ------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AutorResponseDto>>> GetAutores()
        {
            var autores = await _context.Autores
                .Select(a => new AutorResponseDto { Id = a.Id, NombreCompleto = a.NombreCompleto })
                .ToListAsync();

            return Ok(autores);
        }

        // ------------------------------------------------------------------
        // 2. READ SINGLE: Obtener autor por ID (Accesible por Lector y Admin)
        // ------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<AutorResponseDto>> GetAutor(int id)
        {
            var autor = await _context.Autores.FindAsync(id);

            if (autor == null)
            {
                // Devuelve un 404 si no se encuentra el autor
                return NotFound();  // Si el autor no existe, devuelve un 404 Not Found
            }

            return Ok(new AutorResponseDto { Id = autor.Id, NombreCompleto = autor.NombreCompleto });
        }

        // ------------------------------------------------------------------
        // 3. CREATE: Crear un nuevo autor (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpPost]
        // Restricción por Rol: Solo Administradores
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<ActionResult<AutorResponseDto>> PostAutor([FromBody] AutorCreateDto autorDto)
        {
            var autor = new Autor
            {
                NombreCompleto = autorDto.NombreCompleto
            };

            _context.Autores.Add(autor);
            await _context.SaveChangesAsync();

            var responseDto = new AutorResponseDto { Id = autor.Id, NombreCompleto = autor.NombreCompleto };
            return CreatedAtAction(nameof(GetAutor), new { id = autor.Id }, responseDto);
        }

        // ------------------------------------------------------------------
        // 4. UPDATE: Actualizar un autor (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpPut("{id}")]
        // Restricción por Rol
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<IActionResult> PutAutor(int id, [FromBody] AutorUpdateDto autorDto)
        {
            var autor = await _context.Autores.FindAsync(id);
            if (autor == null)
            {
                return NotFound();
            }

            autor.NombreCompleto = autorDto.NombreCompleto;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Autores.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // 204 Success
        }

        // ------------------------------------------------------------------
        // 5. DELETE: Eliminar un autor (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpDelete("{id}")]
        // Restricción por Rol
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<IActionResult> DeleteAutor(int id)
        {
            var autor = await _context.Autores.FindAsync(id);
            if (autor == null)
            {
                return NotFound();
            }

            // Nota: La eliminación de un autor podría fallar si está asociado a libros (Foreign Key constraint).
            // Para un sistema robusto, se debe manejar este error o implementar soft-delete.
            _context.Autores.Remove(autor);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 Success
        }
    }
}
