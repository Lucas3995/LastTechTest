using System.Linq;
using System.Reflection;

using FluentAssertions;

using LastTechTest.API;
using LastTechTest.Aplicacao.Common.Services;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Persistencia;
using LastTechTest.Persistencia.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LastTechTest.Testes.Integration;

/// <summary>Violation #3 (DIP): Verifies IAnticipationAuditService is registered by environment (Testing → NoOp; Production → AnticipationAuditService).</summary>
[Trait("Category", "Integration")]
public sealed class AnticipationAuditServiceRegistrationTests
{
    [Fact]
    public void When_Environment_Is_Testing_Should_Resolve_NoOpAnticipationAuditService()
    {
        using var factory = new CustomWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var audit = scope.ServiceProvider.GetRequiredService<IAnticipationAuditService>();

        audit.Should().BeOfType<NoOpAnticipationAuditService>();
    }

    [Fact]
    public void When_Environment_Is_Production_Should_Resolve_AnticipationAuditService()
    {
        using var factory = new ProductionWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var audit = scope.ServiceProvider.GetRequiredService<IAnticipationAuditService>();

        audit.Should().BeOfType<AnticipationAuditService>();
    }
}

/// <summary>WebApplicationFactory with environment set to Production so DI registers AnticipationAuditService. Uses in-memory SQLite so host starts without a real DB.</summary>
file sealed class ProductionWebApplicationFactory : WebApplicationFactory<ProgramEntry>
{
    private static readonly SqliteConnection ProductionTestConnection;

    static ProductionWebApplicationFactory()
    {
        ProductionTestConnection = new SqliteConnection("Data Source=:memory:");
        ProductionTestConnection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        var testDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var apiProjectDir = Path.GetFullPath(Path.Combine(testDir, "..", "..", "..", "..", "LastTechTest.API"));
        builder.UseContentRoot(apiProjectDir);
        builder.ConfigureServices(services =>
        {
            var toRemove = services.Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) || d.ServiceType == typeof(ApplicationDbContext)).ToList();
            foreach (var d in toRemove) services.Remove(d);
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(ProductionTestConnection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { /* keep connection alive for test run */ }
        base.Dispose(disposing);
    }
}
