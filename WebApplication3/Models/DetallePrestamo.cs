// Models/DetallePrestamo.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication3.Models;

namespace WebApplication3.Models
{
    public class DetallePrestamo
    {
        [Key]
        public int Id { get; set; }

        public int Cantidad { get; set; } // Puede ser 1, pero se incluye por si se presta más de 1 ejemplar

        // Claves Foráneas
        public int PrestamoId { get; set; }
        public int LibroId { get; set; }

        // Propiedades de Navegación

        // 1. Relación con Préstamo (Muchos a Uno)
        public Prestamo Prestamo { get; set; }

        // 2. Relación con Libro (Muchos a Uno)
        public Libro Libro { get; set; }

        // 3. Relación con Multa (Uno a Uno - Opcional)
        public Multa Multa { get; set; }
    }
}