using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Editorial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; }

        public string Direccion { get; set; }

        // Relación: Una Editorial publica muchos Libros
        public ICollection<Libro> Libros { get; set; }
    }
}
