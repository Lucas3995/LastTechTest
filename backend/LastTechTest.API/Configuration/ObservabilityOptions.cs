namespace LastTechTest.API.Configuration;

/// <summary>
/// Strongly-typed options for the "Observability" section in appsettings.
/// Follows the same pattern as <see cref="AnticipationCalculationOptions"/>.
/// Default values are intentionally safe (disabled) so observability is opt-in per environment.
/// </summary>
public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    /// <summary>Master toggle. When false no OpenTelemetry provider is registered.</summary>
    public bool Enabled { get; set; } = false;

    /// <summary>Service name reported in traces and metrics.</summary>
    public string ServiceName { get; set; } = "LastTechTest.API";

    /// <summary>Which exporter to use. Defaults to Console for local development.</summary>
    public ObservabilityExporter Exporter { get; set; } = ObservabilityExporter.Console;

    /// <summary>OTLP endpoint (used only when Exporter = Otlp).</summary>
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";
}

/// <summary>Supported OpenTelemetry exporters.</summary>
public enum ObservabilityExporter
{
    /// <summary>Writes spans/metrics to stdout — useful for local development.</summary>
    Console,

    /// <summary>Sends spans/metrics over OTLP gRPC to <see cref="ObservabilityOptions.OtlpEndpoint"/>.</summary>
    Otlp,

    /// <summary>Exposes a Prometheus scraping endpoint (<c>/metrics</c>). Valid only for metrics.</summary>
    Prometheus
}
