// DTOs/MultaDto.cs

using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs
{
    // DTO para actualizar el estado de la multa (ej: marcar como pagada)
    public class MultaUpdateEstadoDto
    {
        [Required]
        [MaxLength(50)]
        public string Estado { get; set; } // Ej: "Pagada", "Anulada"

        // La FechaPago se registrará en el backend si el estado es "Pagada"
    }

    public class MultaResponseDto
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public DateTime? FechaPago { get; set; }
        public string Estado { get; set; }

        // Datos relacionados
        public string UsuarioId { get; set; }
        public string UsuarioUsername { get; set; }
        public int? DetallePrestamoId { get; set; }
    }
}