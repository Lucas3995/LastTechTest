using System.Reflection;

using LastTechTest.API;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace LastTechTest.Testes;

/// <summary>
/// Sets the content root to the API project directory so E2E tests work both on the host
/// (e.g. repo root) and in Docker (e.g. /src). Without this, the host may resolve content root
/// to a path that only exists in Docker (e.g. /src/backend/LastTechTest.API/) and throw
/// DirectoryNotFoundException.
/// Sets environment to Testing so the API uses a fresh SQLite file per run (Migrate() applies cleanly).
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<ProgramEntry>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Resolve API project dir: from test output (e.g. .../LastTechTest.Testes/bin/Release/net10.0)
        // go up to backend/ then into LastTechTest.API. Works on host and in Docker.
        var testDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var apiProjectDir = Path.GetFullPath(Path.Combine(testDir, "..", "..", "..", "..", "LastTechTest.API"));
        builder.UseContentRoot(apiProjectDir);
    }
}