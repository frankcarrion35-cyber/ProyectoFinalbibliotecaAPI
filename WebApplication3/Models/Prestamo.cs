// Models/Prestamo.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication3.Models;

namespace WebApplication3.Models
{
    public class Prestamo
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaPrestamo { get; set; } = DateTime.UtcNow;

        public DateTime FechaDevolucionPrevista { get; set; }

        public DateTime? FechaDevolucionReal { get; set; }

        // Clave Foránea del Usuario
        public string UsuarioId { get; set; } // Tipo string porque IdentityUser usa string

        // Propiedades de Navegación

        // 1. Relación con Usuario (Muchos a Uno)
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        // 2. Relación con Detalles de Préstamo (Uno a Muchos)
        public ICollection<DetallePrestamo> Detalles { get; set; }
    }
}