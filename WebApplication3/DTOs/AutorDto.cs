// DTOs/AutorDto.cs

using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs
{
    public class AutorCreateDto
    {
        [Required(ErrorMessage = "El nombre completo del autor es obligatorio")]
        [MaxLength(150)]
        public string NombreCompleto { get; set; }
    }

    public class AutorUpdateDto : AutorCreateDto
    {
        // Hereda NombreCompleto
    }

    public class AutorResponseDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
    }
}