// DTOs/EditorialDto.cs

using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs
{
    public class EditorialCreateDto
    {
        [Required(ErrorMessage = "El nombre de la editorial es obligatorio")]
        [MaxLength(150)]
        public string Nombre { get; set; }

        public string Direccion { get; set; }
    }

    public class EditorialUpdateDto : EditorialCreateDto
    {
        // Hereda Nombre y Direccion
    }

    public class EditorialResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
    }
}