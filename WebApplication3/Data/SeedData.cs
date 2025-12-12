// Data/SeedData.cs
using Microsoft.AspNetCore.Identity;
using WebApplication3.Models;

namespace WebApplication3.Data
{
    public static class SeedData
    {
        // CORRECCIÓN: Usar 'const' para resolver CS0182
        public const string AdminRole = "Administrador";
        public const string LectorRole = "Lector";

        public static async Task InitializeRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRole));
            }
            if (!await roleManager.RoleExistsAsync(LectorRole))
            {
                await roleManager.CreateAsync(new IdentityRole(LectorRole));
            }
        }

        public static async Task InitializeAdminUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();

            if (await userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new Usuario
                {
                    UserName = "admin",
                    Email = "admin@biblioteca.com",
                    NombreCompleto = "Administrador del Sistema",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123*");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, AdminRole);
                }
            }
        }
    }
}