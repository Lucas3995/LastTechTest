using System.Diagnostics;

using FluentAssertions;

using Serilog;
using Serilog.Core;
using Serilog.Enrichers.OpenTelemetry;
using Serilog.Events;

namespace LastTechTest.Testes.Unit;

/// <summary>
/// U-SLOG block — unit tests for the Serilog OpenTelemetry enricher
/// (<c>.Enrich.WithOpenTelemetry()</c> from <c>Serilog.Enrichers.OpenTelemetry</c>).
/// Verifies that when a <see cref="System.Diagnostics.Activity"/> is active,
/// log events are automatically decorated with non-null <c>TraceId</c> and <c>SpanId</c>
/// properties, enabling log-trace correlation (CA-RO1-1).
///
/// These tests are self-contained: no host, no WebApplicationFactory, no OTEL SDK providers.
/// They exercise the enricher directly via a <see cref="LoggerConfiguration"/> built in-process.
/// </summary>
[Trait("Category", "Unit")]
public sealed class SerilogOpenTelemetryEnrichmentTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a Serilog <see cref="ILogger"/> that writes to the supplied
    /// <paramref name="sink"/> and enriches every event with both the
    /// OpenTelemetry TraceId and SpanId enrichers.
    /// (<c>Serilog.Enrichers.OpenTelemetry</c> 1.0.1 exposes two separate
    /// extension methods — <c>WithOpenTelemetryTraceId()</c> and
    /// <c>WithOpenTelemetrySpanId()</c> — rather than a single
    /// <c>WithOpenTelemetry()</c> call.)
    /// </summary>
    private static ILogger BuildLogger(UnitCapturingSerilogSink sink) =>
        new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.With(new OpenTelemetryTraceIdEnricher())
            .Enrich.With(new OpenTelemetrySpanIdEnricher())
            .WriteTo.Sink(sink)
            .CreateLogger();

    /// <summary>
    /// Starts a new <see cref="Activity"/> using only the standard
    /// <c>System.Diagnostics</c> API (no OTEL SDK required).
    /// Sets <see cref="Activity.Current"/> so the enricher can read the context.
    /// </summary>
    private static Activity StartTestActivity(string name = "test-span")
    {
        var activity = new Activity(name);
        activity.Start();   // sets Activity.Current
        return activity;    // caller must Stop() / Dispose()
    }

    // ──────────────────────────────────────────────────────────────────────────
    // U6 — TraceId present when an Activity is active (CA-RO1-1)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void U6_SerilogLog_WhenActivityIsActive_ShouldContainNonNullTraceId()
    {
        // Arrange
        var sink = new UnitCapturingSerilogSink();
        var logger = BuildLogger(sink);

        using var activity = StartTestActivity("u6-span");

        // Act
        logger.Information("U6 — test message with active Activity");

        // Stop Activity before assertions so Activity.Current is restored.
        activity.Stop();

        // Assert
        sink.Events.Should().NotBeEmpty("the logger must have captured at least one event");

        var logEvent = sink.Events[0];
        logEvent.Properties.Should().ContainKey("TraceId",
            "the OpenTelemetry enricher must add a TraceId property when Activity.Current is non-null");

        var traceIdValue = logEvent.Properties["TraceId"].ToString().Trim('"');
        traceIdValue.Should().NotBeNullOrWhiteSpace(
            "TraceId must carry the hex-encoded trace identifier from Activity.Current");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // U7 — SpanId present when an Activity is active (CA-RO1-1)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void U7_SerilogLog_WhenActivityIsActive_ShouldContainNonNullSpanId()
    {
        // Arrange
        var sink = new UnitCapturingSerilogSink();
        var logger = BuildLogger(sink);

        using var activity = StartTestActivity("u7-span");

        // Act
        logger.Information("U7 — test message with active Activity");

        activity.Stop();

        // Assert
        sink.Events.Should().NotBeEmpty();

        var logEvent = sink.Events[0];
        logEvent.Properties.Should().ContainKey("SpanId",
            "the OpenTelemetry enricher must add a SpanId property when Activity.Current is non-null");

        var spanIdValue = logEvent.Properties["SpanId"].ToString().Trim('"');
        spanIdValue.Should().NotBeNullOrWhiteSpace(
            "SpanId must carry the hex-encoded span identifier from Activity.Current");
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// Internal helper — UnitCapturingSerilogSink
// Prefixed "Unit" to avoid name collision with the identical helper declared
// (as internal) in the Integration namespace inside ObservabilityIntegrationTests.cs.
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Minimal in-memory Serilog sink that collects <see cref="LogEvent"/> instances
/// for assertion in unit tests. No external dependencies (~10 lines; plan decision D5).
/// </summary>
internal sealed class UnitCapturingSerilogSink : ILogEventSink
{
    private readonly List<LogEvent> _events = new();

    public IReadOnlyList<LogEvent> Events => _events;

    public void Emit(LogEvent logEvent) => _events.Add(logEvent);
}
