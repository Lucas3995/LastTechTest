using FluentAssertions;

using LastTechTest.API;
using LastTechTest.Dominio.Authorization;
using LastTechTest.Persistencia;

using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LastTechTest.Testes.Integration;

[Trait("Category", "Integration")]
public class IdentitySeederIntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;

    public IdentitySeederIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));

        services
            .AddIdentityCore<IdentityUser<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task Seeder_Should_Create_Roles_And_Admin_User()
    {
        await IdentitySeeder.InitializeAsync(_provider);

        using var scope = _provider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();

        (await roleManager.RoleExistsAsync(KnownRoles.Admin)).Should().BeTrue();
        (await roleManager.RoleExistsAsync(KnownRoles.Creator)).Should().BeTrue();
        (await roleManager.RoleExistsAsync(KnownRoles.Analista)).Should().BeTrue();

        var admin = await userManager.FindByEmailAsync("usu_acesso_total@example.com");
        admin.Should().NotBeNull();
        (await userManager.IsInRoleAsync(admin!, KnownRoles.Admin)).Should().BeTrue();
    }

    [Fact]
    public async Task Seeder_Should_Assign_Creator_To_Users_Without_Roles()
    {
        using var scope = _provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();

        var userWithoutRole = new IdentityUser<Guid>
        {
            UserName = "no-role@example.com",
            Email = "no-role@example.com",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(userWithoutRole, "StrongPass1!");
        createResult.Succeeded.Should().BeTrue();

        (await userManager.GetRolesAsync(userWithoutRole)).Should().BeEmpty();

        await IdentitySeeder.InitializeAsync(_provider);

        var rolesAfterSeed = await userManager.GetRolesAsync(userWithoutRole);
        rolesAfterSeed.Should().ContainSingle()
            .Which.Should().Be(KnownRoles.Creator);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
    }
}