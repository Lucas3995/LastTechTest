using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace LastTechTest.API.Configuration;

/// <summary>
/// Extension method that wires up OpenTelemetry tracing and metrics into the host DI container.
/// Guards: no-op when <c>Observability:Enabled = false</c> OR when the environment is "Testing",
/// so existing tests and CI runs are never affected by OTEL overhead.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Registers OpenTelemetry TracerProvider and MeterProvider when observability is enabled
    /// and the environment is not "Testing".
    /// Call this after all <c>builder.Services.*</c> registrations, before <c>builder.Build()</c>.
    /// </summary>
    public static IHostApplicationBuilder AddLastTechTestObservability(
        this IHostApplicationBuilder builder)
    {
        var options = builder.Configuration
            .GetSection(ObservabilityOptions.SectionName)
            .Get<ObservabilityOptions>() ?? new ObservabilityOptions();

        // R1 — toggle + defence-in-depth: Testing environment always skips registration.
        if (!options.Enabled || builder.Environment.IsEnvironment("Testing"))
            return builder;

        var resource = ResourceBuilder.CreateDefault()
            .AddService(options.ServiceName);

        builder.Services
            .AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // R4 — SetDbQueryParameters is internal in OpenTelemetry.Instrumentation.EntityFrameworkCore
                    // 1.15.0-beta.1 and cannot be set via public API yet. SQL query parameters are NOT
                    // captured by default, which satisfies R4 (no raw SQL exposure). Revisit when the
                    // property is promoted to public in a stable release.
                    .AddEntityFrameworkCoreInstrumentation()
                    // R3 — named source reserved for future custom spans in the API layer
                    .AddSource("LastTechTest.API");

                tracing.AddTracingExporter(options, builder.Environment.IsEnvironment("Testing"));
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("LastTechTest.API");

                metrics.AddMetricsExporter(options);
            });

        return builder;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Private helpers — one for tracing, one for metrics, because
    // TracerProviderBuilder and MeterProviderBuilder are unrelated types.
    // ──────────────────────────────────────────────────────────────────────────

    private static TracerProviderBuilder AddTracingExporter(
        this TracerProviderBuilder tracing,
        ObservabilityOptions options,
        bool isTesting)
    {
        if (isTesting)
            return tracing;

        return options.Exporter switch
        {
            ObservabilityExporter.Console =>
                tracing.AddConsoleExporter(),

            ObservabilityExporter.Otlp =>
                tracing.AddOtlpExporter(o => o.Endpoint = new Uri(options.OtlpEndpoint)),

            // Prometheus is metrics-only; silently ignore for tracing.
            _ => tracing
        };
    }

    private static MeterProviderBuilder AddMetricsExporter(
        this MeterProviderBuilder metrics,
        ObservabilityOptions options)
    {
        return options.Exporter switch
        {
            ObservabilityExporter.Console =>
                metrics.AddConsoleExporter(),

            ObservabilityExporter.Otlp =>
                metrics.AddOtlpExporter(o => o.Endpoint = new Uri(options.OtlpEndpoint)),

            ObservabilityExporter.Prometheus =>
                metrics.AddPrometheusExporter(),

            _ => metrics
        };
    }
}