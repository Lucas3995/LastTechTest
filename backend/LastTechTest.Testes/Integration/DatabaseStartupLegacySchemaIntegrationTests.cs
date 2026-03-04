using FluentAssertions;

using LastTechTest.API;
using LastTechTest.Persistencia;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LastTechTest.Testes.Integration;

/// <summary>
/// Ensures that when the database was created with EnsureCreated() (no migration history),
/// e.g. Docker volume from before migrations existed, the API startup still succeeds and
/// the AnticipationRequests table is created. Prevents containerization regression.
/// </summary>
[Trait("Category", "Integration")]
public sealed class DatabaseStartupLegacySchemaIntegrationTests
{
    private const string LegacySchemaSql = """
        CREATE TABLE IF NOT EXISTS "Users" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,
            "Email" TEXT NOT NULL,
            "PasswordHash" TEXT NOT NULL,
            "Status" INTEGER NOT NULL,
            "CreatedAtUtc" TEXT NOT NULL,
            "LastLoginAtUtc" TEXT NULL
        );
        CREATE TABLE IF NOT EXISTS "UserMfa" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_UserMfa" PRIMARY KEY,
            "UserId" TEXT NOT NULL,
            "SecretKey" TEXT NOT NULL,
            "Enabled" INTEGER NOT NULL,
            "CreatedAtUtc" TEXT NOT NULL,
            CONSTRAINT "FK_UserMfa_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
        );
        CREATE TABLE IF NOT EXISTS "UserTokens" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_UserTokens" PRIMARY KEY,
            "UserId" TEXT NOT NULL,
            "Value" TEXT NOT NULL,
            "Type" INTEGER NOT NULL,
            "CreatedAtUtc" TEXT NOT NULL,
            "ExpiresAtUtc" TEXT NOT NULL,
            "Revoked" INTEGER NOT NULL,
            CONSTRAINT "FK_UserTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
        );
        """;

    [Fact]
    public void EnsureSchema_WhenDatabaseHasLegacySchemaOnly_Should_NotThrow_And_CreateAnticipationRequestsTable()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = LegacySchemaSql;
            cmd.ExecuteNonQuery();
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var db = new ApplicationDbContext(options))
        {
            var act = () => DatabaseStartup.EnsureSchema(db, isTesting: false);
            act.Should().NotThrow();
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='AnticipationRequests'";
            var name = cmd.ExecuteScalar() as string;
            name.Should().Be("AnticipationRequests", "fallback should have created AnticipationRequests when Migrate() failed with 'already exists'");
        }

        connection.Dispose();
    }
}