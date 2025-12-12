// DTOs/PrestamoDto.cs

using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs
{
    // DTO para manejar los libros que forman parte de este préstamo
    public class DetallePrestamoCreateDto
    {
        [Required(ErrorMessage = "El ID del libro es obligatorio.")]
        public int LibroId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Se debe prestar al menos 1 ejemplar.")] // Límite arbitrario
        public int Cantidad { get; set; }
    }

    // DTO principal para la creación del Préstamo
    public class PrestamoCreateDto
    {
        [Required(ErrorMessage = "La fecha prevista de devolución es obligatoria.")]
        public DateTime FechaDevolucionPrevista { get; set; }

        [Required(ErrorMessage = "Se debe especificar al menos un libro.")]
        public List<DetallePrestamoCreateDto> Detalles { get; set; }
    }

    // DTO de respuesta para el Detalle del Préstamo
    public class DetallePrestamoResponseDto
    {
        public int Id { get; set; }
        public int LibroId { get; set; }
        public string LibroTitulo { get; set; }
        public int Cantidad { get; set; }
        public int? MultaId { get; set; }
    }

    // DTO de respuesta para el Préstamo completo
    public class PrestamoResponseDto
    {
        public int Id { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaDevolucionPrevista { get; set; }
        public DateTime? FechaDevolucionReal { get; set; }

        // Datos relacionados
        public string UsuarioId { get; set; }
        public string UsuarioUsername { get; set; }

        public List<DetallePrestamoResponseDto> Detalles { get; set; }
    }
}