using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        // Relación: Una Categoría puede tener muchos Libros
        public ICollection<Libro> Libros { get; set; }
    }
}
