// Program.cs
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using Microsoft.AspNetCore.Identity;
using WebApplication3.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApplication3.Services;
using Npgsql.EntityFrameworkCore.PostgreSQL; // Para despliegue en Render/PostgreSQL


namespace WebApplication3
{
    public class Program
    {
        // CORRECCIÓN: Método principal asíncrono
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- CONFIGURACIÓN DE DATOS Y SEGURIDAD ---

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Lógica de BD adaptativa (SQL Server local / PostgreSQL en producción)
            if (builder.Environment.IsProduction())
            {
                // Se asume que en producción se usa PostgreSQL (ej: Render/Railway)
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(connectionString));
            }
            else
            {
                // SQL Server para desarrollo local
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            // 2. Configurar Identity
            builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // 3. Configurar JWT
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];
            var jwtKey = builder.Configuration["Jwt:Key"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

            // 4. Agregar Autorización y Roles
            builder.Services.AddAuthorization();

            // 5. Registrar el servicio de token personalizado (Una sola vez)
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

            // --- SERVICIOS DE LA API ---
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();

            // --- INICIALIZACIÓN DE MIDDLEWARE Y DATOS ---

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Lógica de Inicialización (Roles, Admin, Migraciones)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dbContext = services.GetRequiredService<ApplicationDbContext>();

                    // 1. Aplicar Migraciones (CRÍTICO para producción)
                    if (app.Environment.IsProduction())
                    {
                        dbContext.Database.Migrate();
                    }

                    // 2. Inicializar Roles y el usuario Admin
                    await SeedData.InitializeRolesAsync(services);
                    await SeedData.InitializeAdminUserAsync(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred during DB initialization.");
                }
            }

            await app.RunAsync();
        }
    }
}