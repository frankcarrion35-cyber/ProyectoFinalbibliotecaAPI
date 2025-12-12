// DTOs/CategoriaDto.cs

using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs
{
    public class CategoriaCreateDto
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [MaxLength(100)]
        public string Nombre { get; set; }
    }

    public class CategoriaUpdateDto : CategoriaCreateDto
    {
        // Hereda Nombre
    }

    // Opcionalmente, un DTO para devolver datos (salida)
    public class CategoriaResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}