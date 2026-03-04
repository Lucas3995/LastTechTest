using LastTechTest.Persistencia;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LastTechTest.API;

/// <summary>
/// Ensures database schema at application startup. Handles both normal migrations
/// and the case where the DB was created with EnsureCreated() (e.g. Docker volume from before migrations),
/// so the API starts without "table already exists" when running Migrate().
/// </summary>
public static class DatabaseStartup
{
    private const string CreateAnticipationRequestsIfNotExists = """
        CREATE TABLE IF NOT EXISTS "AnticipationRequests" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_AnticipationRequests" PRIMARY KEY,
            "CreatorId" TEXT NOT NULL,
            "Status" INTEGER NOT NULL,
            "RequestedAmount" TEXT NOT NULL,
            "GrossAmount" TEXT NOT NULL,
            "FeesAmount" TEXT NOT NULL,
            "NetAmount" TEXT NOT NULL,
            "CreatedAtUtc" TEXT NOT NULL,
            "RequestedAtUtc" TEXT NOT NULL
        );
        """;

    private const string AlterAnticipationRequestsAddRequestedAtUtc = """
        ALTER TABLE "AnticipationRequests" ADD COLUMN "RequestedAtUtc" TEXT NOT NULL DEFAULT '0001-01-01 00:00:00';
        """;

    private static void RunLegacyFallback(ApplicationDbContext db)
    {
        db.Database.ExecuteSqlRaw(CreateAnticipationRequestsIfNotExists);
        try
        {
            db.Database.ExecuteSqlRaw(AlterAnticipationRequestsAddRequestedAtUtc);
        }
        catch (SqliteException alterEx) when (alterEx.Message?.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Column already exists (e.g. from a previous run of this fallback).
        }
    }

    /// <summary>
    /// Ensures schema exists: EnsureCreated() when testing (in-memory), Migrate() otherwise.
    /// If Migrate() fails because tables already exist (legacy EnsureCreated DB) or model has pending changes, runs legacy fallback.
    /// </summary>
    public static void EnsureSchema(ApplicationDbContext db, bool isTesting)
    {
        if (isTesting)
        {
            db.Database.EnsureCreated();
            return;
        }

        try
        {
            db.Database.Migrate();
        }
        catch (SqliteException ex) when (ex.Message?.Contains("already exists", StringComparison.OrdinalIgnoreCase) == true)
        {
            RunLegacyFallback(db);
        }
        catch (InvalidOperationException ex) when (ex.Message?.Contains("pending", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Model has newer migrations than the DB (e.g. legacy DB with no __EFMigrationsHistory). Apply fallback.
            RunLegacyFallback(db);
        }
    }
}