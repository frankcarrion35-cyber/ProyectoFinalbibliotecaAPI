// Models/Multa.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication3.Models;

namespace WebApplication3.Models
{
    public class Multa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaPago { get; set; }

        [MaxLength(50)]
        public string Estado { get; set; } // Ej: Pendiente, Pagada, Anulada

        // Claves Foráneas
        public string UsuarioId { get; set; } // Tipo string
        public int? DetallePrestamoId { get; set; } // Relación 1 a 1 opcional con DetallePrestamo

        // Propiedades de Navegación

        // 1. Relación con Usuario (Muchos a Uno)
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        // 2. Relación con DetallePrestamo (Uno a Uno)
        public DetallePrestamo DetallePrestamo { get; set; }
    }
}