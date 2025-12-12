// Models/Reserva.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication3.Models;

namespace WebApplication3.Models
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaReserva { get; set; } = DateTime.UtcNow;

        public DateTime FechaExpiracion { get; set; }

        [MaxLength(50)]
        public string Estado { get; set; } // Ej: Pendiente, Completada, Cancelada

        // Claves Foráneas
        public string UsuarioId { get; set; } // Tipo string
        public int LibroId { get; set; }

        // Propiedades de Navegación

        // 1. Relación con Usuario (Muchos a Uno)
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        // 2. Relación con Libro (Muchos a Uno)
        public Libro Libro { get; set; }
    }
}