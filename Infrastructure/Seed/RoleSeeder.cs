using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Seed;

public class RoleSeeder
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<User> userManager)
    {
        string[] roles = [UserRoles.Admin, UserRoles.Manager, UserRoles.User];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = "akmalzodaardasher@gmail.com";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Ardasher",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Ardasher_123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, UserRoles.Admin);
            }
        }
    }
}