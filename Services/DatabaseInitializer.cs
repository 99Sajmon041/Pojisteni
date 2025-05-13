using Microsoft.AspNetCore.Identity;
using Pojisteni.Models;

namespace Pojisteni.Services
{
    public class DatabaseInitializer
    {
        public static async Task InitializeDataAsync(UserManager<ApplicationUser>? userManager, RoleManager<IdentityRole>? roleManager)
        {
            if(userManager == null || roleManager == null)
            {
                Console.WriteLine("Uživatel, nebo role exestuje !");
                return;
            }

            var exists = await roleManager.RoleExistsAsync(Role.admin);
            if(!exists)
            {
                Console.WriteLine($"Role: {Role.admin} definovaná a zároveň se vytvoří");
                await roleManager.CreateAsync(new IdentityRole(Role.admin));
            }

            exists = await roleManager.RoleExistsAsync(Role.client);
            if (!exists)
            {
                Console.WriteLine($"Role: {Role.client} není definovaná a zároveň se vytvoří");
                await roleManager.CreateAsync(new IdentityRole(Role.client));
            }
            else
            {
                Console.WriteLine($"Role: {Role.client} již existuje");
            }

            var adminUser = await userManager.GetUsersInRoleAsync(Role.admin);
            if(adminUser.Any())
            {
                Console.WriteLine($"Role: {Role.admin} již existuje");
                return;
            }

            var user = new ApplicationUser()
            {
                Firstname = "admin",
                Lastname = "admin",
                Email = "simon8durak@gmail.com",
                UserName = "simon8durak@gmail.com"
            };

            string password = "Durasi99";

            var result = await userManager.CreateAsync(user, password);
            if(result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, Role.admin);
                Console.WriteLine($"Role admina vytvořena. Email uživatele: {user.Email}");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Chyba při vytváření uživatele: {user.Email} - {error.Description}");
                }
            }
        }
    }
}
