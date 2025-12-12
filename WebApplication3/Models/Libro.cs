using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication3.Models
{
    public class Libro
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Titulo { get; set; }

        public string ISBN { get; set; }

        public int AnioPublicacion { get; set; }

        public int CantidadDisponible { get; set; }

        // Claves Foráneas
        public int CategoriaId { get; set; }
        public int EditorialId { get; set; }

        // Propiedades de Navegación (Relaciones)

        // 1. Relación con Categoría (Muchos a Uno)
        public Categoria Categoria { get; set; }

        // 2. Relación con Editorial (Muchos a Uno)
        public Editorial Editorial { get; set; }

        // 3. Relación con Autor (Muchos a Muchos)
        public ICollection<Autor> Autores { get; set; }

        // 4. Relación con Prestamos (Uno a Muchos)
        public ICollection<DetallePrestamo> DetallesPrestamo { get; set; }

        // 5. Relación con Reservas (Uno a Muchos)
        public ICollection<Reserva> Reservas { get; set; }
    }
}
