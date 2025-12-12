// Data/ApplicationDbContext.cs (ACTUALIZADO)
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Models;

namespace WebApplication3.Data
{
    // Citar que el DbContext ahora usa nuestra clase Usuario personalizada
    public class ApplicationDbContext : IdentityDbContext<Usuario, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Definición de las 9 entidades restantes
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Prestamo> Prestamos { get; set; }
        public DbSet<DetallePrestamo> DetallesPrestamo { get; set; }
        public DbSet<Editorial> Editoriales { get; set; }
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Multa> Multas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Renombrar las tablas de Identity por claridad (opcional, pero buena práctica)
            builder.Entity<Usuario>().ToTable("Usuarios");
            builder.Entity<IdentityRole>().ToTable("Roles");

            // Configurar la relación Muchos a Muchos entre Libro y Autor
            builder.Entity<Libro>()
                .HasMany(l => l.Autores)
                .WithMany(a => a.Libros)
                .UsingEntity(j => j.ToTable("LibroAutor")); // Nombre de la tabla de unión

            // Configurar la relación Uno a Uno entre Multa y DetallePrestamo
            builder.Entity<DetallePrestamo>()
                .HasOne(dp => dp.Multa)
                .WithOne(m => m.DetallePrestamo)
                .HasForeignKey<Multa>(m => m.DetallePrestamoId)
                .IsRequired(false); // La multa es opcional

            // Asegurar que el UsuarioId en Prestamo, Reserva y Multa coincida con el tipo de clave de IdentityUser (string)
            builder.Entity<Prestamo>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Prestamos)
                .HasForeignKey(p => p.UsuarioId);

            builder.Entity<Reserva>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.Reservas)
                .HasForeignKey(r => r.UsuarioId);

            builder.Entity<Multa>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.Multas)
                .HasForeignKey(m => m.UsuarioId);
        }
    }
}