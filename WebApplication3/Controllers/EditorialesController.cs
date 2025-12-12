// Controllers/EditorialesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.DTOs;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    // Requerir autenticación JWT para todos los endpoints
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EditorialesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EditorialesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------
        // 1. READ ALL: Obtener todas las editoriales
        // ------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EditorialResponseDto>>> GetEditoriales()
        {
            var editoriales = await _context.Editoriales
                .Select(e => new EditorialResponseDto { Id = e.Id, Nombre = e.Nombre, Direccion = e.Direccion })
                .ToListAsync();

            return Ok(editoriales);
        }

        // ------------------------------------------------------------------
        // 2. READ SINGLE: Obtener editorial por ID
        // ------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<EditorialResponseDto>> GetEditorial(int id)
        {
            var editorial = await _context.Editoriales.FindAsync(id);

            if (editorial == null)
            {
                return NotFound();
            }

            return Ok(new EditorialResponseDto { Id = editorial.Id, Nombre = editorial.Nombre, Direccion = editorial.Direccion });
        }


        // ------------------------------------------------------------------
        // 3. CREATE: Crear una nueva editorial (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<ActionResult<EditorialResponseDto>> PostEditorial([FromBody] EditorialCreateDto editorialDto)
        {
            var editorial = new Editorial
            {
                Nombre = editorialDto.Nombre,
                Direccion = editorialDto.Direccion
            };

            _context.Editoriales.Add(editorial);
            await _context.SaveChangesAsync();

            var responseDto = new EditorialResponseDto { Id = editorial.Id, Nombre = editorial.Nombre, Direccion = editorial.Direccion };
            return CreatedAtAction(nameof(GetEditorial), new { id = editorial.Id }, responseDto);
        }

        // ------------------------------------------------------------------
        // 4. UPDATE: Actualizar una editorial (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpPut("{id}")]
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<IActionResult> PutEditorial(int id, [FromBody] EditorialUpdateDto editorialDto)
        {
            var editorial = await _context.Editoriales.FindAsync(id);
            if (editorial == null)
            {
                return NotFound();
            }

            editorial.Nombre = editorialDto.Nombre;
            editorial.Direccion = editorialDto.Direccion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Editoriales.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // ------------------------------------------------------------------
        // 5. DELETE: Eliminar una editorial (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpDelete("{id}")]
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<IActionResult> DeleteEditorial(int id)
        {
            var editorial = await _context.Editoriales.FindAsync(id);
            if (editorial == null)
            {
                return NotFound();
            }

            _context.Editoriales.Remove(editorial);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}