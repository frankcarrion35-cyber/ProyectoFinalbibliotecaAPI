// DTOs/ReservaDto.cs

using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs
{
    public class ReservaCreateDto
    {
        [Required(ErrorMessage = "El ID del libro es obligatorio.")]
        public int LibroId { get; set; }

        // No pedimos UsuarioId en el DTO de Creación, lo obtenemos del JWT del usuario logueado.

        // Para simplificar la lógica de negocio, se puede establecer la FechaExpiracion en el backend.
    }

    // Opcionalmente, un DTO para actualizar el estado (ej: cancelar)
    public class ReservaUpdateEstadoDto
    {
        [Required]
        [MaxLength(50)]
        public string Estado { get; set; } // Ej: "Cancelada", "Completada"
    }

    public class ReservaResponseDto
    {
        public int Id { get; set; }
        public DateTime FechaReserva { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public string Estado { get; set; }

        // Datos relacionados
        public int LibroId { get; set; }
        public string LibroTitulo { get; set; }
        public string UsuarioId { get; set; }
        public string UsuarioUsername { get; set; }
    }
}