// Models/Usuario.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    // Heredamos de IdentityUser para manejar la autenticación
    public class Usuario : IdentityUser
    {
        [Required]
        public string NombreCompleto { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Relaciones
        public ICollection<Prestamo> Prestamos { get; set; }
        public ICollection<Reserva> Reservas { get; set; }
        public ICollection<Multa> Multas { get; set; }
    }
}