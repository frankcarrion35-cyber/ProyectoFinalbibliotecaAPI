using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Controllers;
using WebApplication3.Data;
using WebApplication3.Models;
using WebApplication3.DTOs;
using Xunit;

namespace WebApplication3.Tests
{
    public class LibrosControllerTests
    {
        private readonly LibrosController _controller;
        private readonly ApplicationDbContext _context;

        public LibrosControllerTests()
        {
            // Configura el DbContext en memoria para simular una base de datos durante las pruebas
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")  // Nombre de la base de datos en memoria
                .Options;

            _context = new ApplicationDbContext(options);

            // Limpiar la base de datos antes de agregar nuevos datos para evitar duplicación
            _context.Libros.RemoveRange(_context.Libros);  // Eliminar cualquier libro previamente agregado
            _context.Categorias.RemoveRange(_context.Categorias);  // Eliminar las categorías previamente agregadas
            _context.Editoriales.RemoveRange(_context.Editoriales);  // Eliminar las editoriales previamente agregadas
            _context.Autores.RemoveRange(_context.Autores);  // Eliminar los autores previamente agregados

            _context.SaveChanges();  // Guardar los cambios para asegurarse de que la base de datos esté limpia

            // Agregar datos de prueba: Categoria, Editorial y Autor
            var categoria = new Categoria { Id = 1, Nombre = "Ficción" };

            // Asegurarse de agregar la propiedad 'Direccion' al crear la editorial
            var editorial = new Editorial { Id = 1, Nombre = "Editorial ABC", Direccion = "Calle Ficticia 123" };
            var autor = new Autor { Id = 1, NombreCompleto = "Autor de prueba" };

            _context.Categorias.Add(categoria);
            _context.Editoriales.Add(editorial);
            _context.Autores.Add(autor);
            _context.SaveChanges();  // Guardar los cambios para los datos de relaciones

            // Agregar un libro de prueba (evitar duplicación en las relaciones)
            _context.Libros.Add(new Libro
            {
                Id = 1,
                Titulo = "Libro de prueba",
                ISBN = "12345",
                AnioPublicacion = 2021,
                CantidadDisponible = 10,
                CategoriaId = categoria.Id,
                EditorialId = editorial.Id,
                Autores = new List<Autor> { autor }  // Relación con el autor (sin duplicados)
            });
            _context.SaveChanges();  // Guardar el libro de prueba

            // Crear una instancia del controlador pasando el DbContext
            _controller = new LibrosController(_context);
        }

        [Fact]
        public async Task GetLibros_ReturnsOkResult()
        {
            // Act
            var result = await _controller.GetLibros();  // Llama al método GetLibros()

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);  // Verifica que el resultado sea OkObjectResult
            var libros = Assert.IsType<List<LibroResponseDto>>(okResult.Value);  // Verifica que el valor sea una lista de libros
            Assert.NotNull(libros);  // Verifica que la lista no sea nula
            Assert.NotEmpty(libros);  // Verifica que la lista no esté vacía
        }

        [Fact]
        public async Task GetLibro_ReturnsNotFoundResult_WhenLibroDoesNotExist()
        {
            // Act
            var result = await _controller.GetLibro(999);  // Llama al método GetLibro con un id que no existe

            // Assert
            Assert.IsType<NotFoundResult>(result);  // Verifica que el resultado sea NotFound (404)
        }

        [Fact]
        public async Task GetLibro_ReturnsOkResult_WhenLibroExists()
        {
            // Act
            var result = await _controller.GetLibro(1);  // Llama al método GetLibro con un id válido

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);  // Verifica que el resultado sea OkObjectResult (200)
            var libro = Assert.IsType<LibroResponseDto>(okResult.Value);  // Verifica que el valor sea un LibroResponseDto
            Assert.NotNull(libro);  // Verifica que el libro no sea nulo
            Assert.Equal(1, libro.Id);  // Verifica que el id sea el esperado
            Assert.Equal("Libro de prueba", libro.Titulo);  // Verifica el título del libro
            Assert.Equal("12345", libro.ISBN);  // Verifica el ISBN
            Assert.Equal(2021, libro.AnioPublicacion);  // Verifica el año de publicación
            Assert.Equal(10, libro.CantidadDisponible);  // Verifica la cantidad disponible
        }

        // También puedes agregar más pruebas para POST, PUT, DELETE, etc.
    }
}
