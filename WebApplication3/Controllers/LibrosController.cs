// Controllers/LibrosController.cs
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
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LibrosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------
        // 1. READ ALL: Obtener todos los libros (Incluye datos relacionados)
        // ------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LibroResponseDto>>> GetLibros()
        {
            var libros = await _context.Libros
                .Include(l => l.Categoria)
                .Include(l => l.Editorial)
                .Include(l => l.Autores) // Relación Muchos a Muchos
                .Select(l => new LibroResponseDto
                {
                    Id = l.Id,
                    Titulo = l.Titulo,
                    ISBN = l.ISBN,
                    AnioPublicacion = l.AnioPublicacion,
                    CantidadDisponible = l.CantidadDisponible,
                    CategoriaId = l.CategoriaId,
                    CategoriaNombre = l.Categoria.Nombre,
                    EditorialId = l.EditorialId,
                    EditorialNombre = l.Editorial.Nombre,
                    Autores = l.Autores.Select(a => new AutorResponseDto
                    {
                        Id = a.Id,
                        NombreCompleto = a.NombreCompleto
                    }).ToList()
                })
                .ToListAsync();

            return Ok(libros);
        }

        // ------------------------------------------------------------------
        // 2. READ SINGLE: Obtener libro por ID
        // ------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<LibroResponseDto>> GetLibro(int id)
        {
            var libro = await _context.Libros
               .Include(l => l.Categoria)
               .Include(l => l.Editorial)
               .Include(l => l.Autores)
               .FirstOrDefaultAsync(l => l.Id == id);

            if (libro == null)
            {
                return NotFound();
            }

            return Ok(new LibroResponseDto
            {
                Id = libro.Id,
                Titulo = libro.Titulo,
                ISBN = libro.ISBN,
                AnioPublicacion = libro.AnioPublicacion,
                CantidadDisponible = libro.CantidadDisponible,
                CategoriaId = libro.CategoriaId,
                CategoriaNombre = libro.Categoria.Nombre,
                EditorialId = libro.EditorialId,
                EditorialNombre = libro.Editorial.Nombre,
                Autores = libro.Autores.Select(a => new AutorResponseDto
                {
                    Id = a.Id,
                    NombreCompleto = a.NombreCompleto
                }).ToList()
            });
        }


        // ------------------------------------------------------------------
        // 3. CREATE: Crear un nuevo libro (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<ActionResult<LibroResponseDto>> PostLibro([FromBody] LibroCreateDto libroDto)
        {
            // 1. Validar la existencia de Categoría y Editorial
            if (!await _context.Categorias.AnyAsync(c => c.Id == libroDto.CategoriaId) ||
                !await _context.Editoriales.AnyAsync(e => e.Id == libroDto.EditorialId))
            {
                return BadRequest("La Categoría o la Editorial especificada no existe.");
            }

            var libro = new Libro
            {
                Titulo = libroDto.Titulo,
                ISBN = libroDto.ISBN,
                AnioPublicacion = libroDto.AnioPublicacion,
                CantidadDisponible = libroDto.CantidadDisponible,
                CategoriaId = libroDto.CategoriaId,
                EditorialId = libroDto.EditorialId,
                Autores = new List<Autor>() // Inicializar la colección
            };

            // 2. Manejar la relación Muchos a Muchos (Autores)
            if (libroDto.AutorIds != null && libroDto.AutorIds.Any())
            {
                // Obtener solo los autores que existen en la BD
                var autoresExistentes = await _context.Autores
                    .Where(a => libroDto.AutorIds.Contains(a.Id))
                    .ToListAsync();

                if (autoresExistentes.Count != libroDto.AutorIds.Count)
                {
                    return BadRequest("Uno o más IDs de autores proporcionados no son válidos.");
                }

                libro.Autores = autoresExistentes;
            }

            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();

            // Mapeo a Response DTO
            var categoria = await _context.Categorias.FindAsync(libro.CategoriaId);
            var editorial = await _context.Editoriales.FindAsync(libro.EditorialId);

            var responseDto = new LibroResponseDto
            {
                Id = libro.Id,
                Titulo = libro.Titulo,
                ISBN = libro.ISBN,
                AnioPublicacion = libro.AnioPublicacion,
                CantidadDisponible = libro.CantidadDisponible,
                CategoriaId = libro.CategoriaId,
                CategoriaNombre = categoria.Nombre,
                EditorialId = libro.EditorialId,
                EditorialNombre = editorial.Nombre,
                Autores = libro.Autores.Select(a => new AutorResponseDto { Id = a.Id, NombreCompleto = a.NombreCompleto }).ToList()
            };

            return CreatedAtAction(nameof(GetLibro), new { id = libro.Id }, responseDto);
        }

        // ------------------------------------------------------------------
        // 4. UPDATE: Actualizar un libro (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpPut("{id}")]
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<IActionResult> PutLibro(int id, [FromBody] LibroUpdateDto libroDto)
        {
            var libro = await _context.Libros
                .Include(l => l.Autores) // Necesario para modificar la colección de Autores
                .FirstOrDefaultAsync(l => l.Id == id);

            if (libro == null)
            {
                return NotFound();
            }

            // 1. Validar la existencia de FKs
            if (!await _context.Categorias.AnyAsync(c => c.Id == libroDto.CategoriaId) ||
                !await _context.Editoriales.AnyAsync(e => e.Id == libroDto.EditorialId))
            {
                return BadRequest("La Categoría o la Editorial especificada no existe.");
            }

            // 2. Actualizar campos simples y FKs
            libro.Titulo = libroDto.Titulo;
            libro.ISBN = libroDto.ISBN;
            libro.AnioPublicacion = libroDto.AnioPublicacion;
            libro.CantidadDisponible = libroDto.CantidadDisponible;
            libro.CategoriaId = libroDto.CategoriaId;
            libro.EditorialId = libroDto.EditorialId;

            // 3. Actualizar la relación Muchos a Muchos (Autores)
            var autoresExistentes = await _context.Autores
                .Where(a => libroDto.AutorIds.Contains(a.Id))
                .ToListAsync();

            if (autoresExistentes.Count != libroDto.AutorIds.Count)
            {
                return BadRequest("Uno o más IDs de autores proporcionados no son válidos.");
            }

            // Reemplazar la colección de autores
            libro.Autores.Clear(); // Elimina los vínculos existentes
            libro.Autores = autoresExistentes; // Agrega los nuevos vínculos

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Libros.Any(e => e.Id == id))
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
        // 5. DELETE: Eliminar un libro (Sólo Administrador)
        // ------------------------------------------------------------------
        [HttpDelete("{id}")]
        [Authorize(Roles = SeedData.AdminRole)]
        public async Task<IActionResult> DeleteLibro(int id)
        {
            var libro = await _context.Libros.FindAsync(id);
            if (libro == null)
            {
                return NotFound();
            }

            // Nota: La eliminación de un libro puede fallar si existen Prestamos/Reservas asociadas.
            _context.Libros.Remove(libro);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}