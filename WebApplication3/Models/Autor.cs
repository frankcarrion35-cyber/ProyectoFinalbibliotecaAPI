using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Autor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string NombreCompleto { get; set; }

        // Relación: Un Autor puede escribir muchos Libros (muchos a muchos, manejado en Libro)
        public ICollection<Libro> Libros { get; set; }
    }
}
