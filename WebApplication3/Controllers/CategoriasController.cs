// Controllers/CategoriasController.cs
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
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------
        // READ: Obtener todas las categorías (Accesible por Lector y Admin)
        
        // ------------------------------------------------------------------
        [HttpGet]
        [AllowAnonymous] // Se permite el acceso sin autenticación si se desea, 
                         // o dejar Authorize para requerir login. Por ahora, dejemos que requiera login.
        public async Task<ActionResult<IEnumerable<CategoriaResponseDto>>> GetCategorias()
        {
            var categorias = await _context.Categorias
                .Select(c => new CategoriaResponseDto { Id = c.Id, Nombre = c.Nombre })
                .ToListAsync();

            if (categorias == null)
            {
                return NotFound();
            }
            return Ok(categorias);
        }

        // ------------------------------------------------------------------
        // READ: Obtener categoría por ID (Accesible por Lector y Admin)
        // ------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaResponseDto>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            return Ok(new CategoriaResponseDto { Id = categoria.Id, Nombre = categoria.Nombre });
        }


        // ------------------------------------------------------------------
        // CREATE: Crear una nueva categoría (Sólo Administrador)
        
        // ------------------------------------------------------------------
        [HttpPost]
        // Restricción por Rol: Solo usuarios con el rol "Administrador" pueden ejecutar esto
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<ActionResult<CategoriaResponseDto>> PostCategoria([FromBody] CategoriaCreateDto categoriaDto)
        {
            var categoria = new Categoria
            {
                Nombre = categoriaDto.Nombre
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            // Devuelve 201 Created con la ubicación del nuevo recurso
            var responseDto = new CategoriaResponseDto { Id = categoria.Id, Nombre = categoria.Nombre };
            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, responseDto);
        }

        // ------------------------------------------------------------------
        // UPDATE: Actualizar una categoría (Sólo Administrador)
        
        // ------------------------------------------------------------------
        [HttpPut("{id}")]
        // Restricción por Rol
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<IActionResult> PutCategoria(int id, [FromBody] CategoriaUpdateDto categoriaDto)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            categoria.Nombre = categoriaDto.Nombre;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categorias.Any(e => e.Id == id))
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
        // DELETE: Eliminar una categoría (Sólo Administrador)
        
        // ------------------------------------------------------------------
        [HttpDelete("{id}")]
        // Restricción por Rol
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 Success
        }
    }
}