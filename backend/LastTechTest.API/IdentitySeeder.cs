using LastTechTest.Dominio.Authorization;
using LastTechTest.Dominio.Entities;
using LastTechTest.Infrastrutura;
using LastTechTest.Persistencia;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LastTechTest.API;

public static class IdentitySeeder
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Garantir que todas as tabelas do modelo (incluindo Identity) existem
        // antes de tentar consultar/seedar roles e usuários.
        await context.Database.EnsureCreatedAsync();

        await EnsureRolesAsync(roleManager);
        await EnsureAdminUserAsync(userManager, context);
        await EnsureExistingUsersHaveCreatorRoleAsync(userManager, context);
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[]
        {
            KnownRoles.Admin,
            KnownRoles.Creator,
            KnownRoles.Analista
        };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }

    private static async Task EnsureAdminUserAsync(
        UserManager<IdentityUser<Guid>> userManager,
        ApplicationDbContext context)
    {
        const string adminEmail = "usu_acesso_total@example.com";
        const string adminPassword = "Acess0@t0ta1";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new IdentityUser<Guid>
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to create admin user: " +
                                                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, KnownRoles.Admin))
        {
            await userManager.AddToRoleAsync(admin, KnownRoles.Admin);
        }

        var existingDomainAdmin = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (existingDomainAdmin is null)
        {
            var domainAdmin = new User();
            domainAdmin.SetEmail(adminEmail);

            var passwordHasher = new PasswordHasher();
            var hash = passwordHasher.HashPassword(domainAdmin, adminPassword);
            domainAdmin.SetPasswordHash(hash);

            await context.Users.AddAsync(domainAdmin);
            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureExistingUsersHaveCreatorRoleAsync(
        UserManager<IdentityUser<Guid>> userManager,
        ApplicationDbContext context)
    {
        var users = userManager.Users.ToList();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Count == 0)
            {
                await userManager.AddToRoleAsync(user, KnownRoles.Creator);
            }
        }
    }
}