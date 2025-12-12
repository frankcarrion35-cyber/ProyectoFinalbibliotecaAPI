// DTOs/LibroDto.cs

using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs
{
    public class LibroCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string Titulo { get; set; }

        public string ISBN { get; set; }

        public int AnioPublicacion { get; set; }

        [Required]
        public int CantidadDisponible { get; set; }

        // Claves Foráneas Requeridas
        [Required]
        public int CategoriaId { get; set; }
        [Required]
        public int EditorialId { get; set; }

        // Relación Muchos a Muchos: Lista de IDs de Autores
        public List<int> AutorIds { get; set; } = new List<int>();
    }

    public class LibroUpdateDto : LibroCreateDto
    {
        // Hereda todas las propiedades del CreateDto
    }

    public class LibroResponseDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string ISBN { get; set; }
        public int AnioPublicacion { get; set; }
        public int CantidadDisponible { get; set; }

        // Datos de las relaciones para mostrar en la respuesta
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; }
        public int EditorialId { get; set; }
        public string EditorialNombre { get; set; }
        public List<AutorResponseDto> Autores { get; set; } // Reutilizamos el DTO de Autor
    }
}