using FluentAssertions;

using LastTechTest.API.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace LastTechTest.Testes.Unit;

/// <summary>
/// U-OTEXT block — unit tests for <see cref="OpenTelemetryExtensions.AddLastTechTestObservability"/>.
/// Verifies DI registration behaviour based on the <c>Observability:Enabled</c> toggle
/// and environment name guard, without spinning up a full WebApplicationFactory.
/// CA-RO1-2, CA-RO1-4, CA-RO1-5.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ObservabilityExtensionsTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a <see cref="HostApplicationBuilder"/> with the given environment and
    /// in-memory configuration overrides, then calls <c>AddLastTechTestObservability()</c>
    /// and returns the built <see cref="IServiceProvider"/>.
    /// </summary>
    private static IServiceProvider BuildProviderWith(
        string environmentName,
        Dictionary<string, string?> configValues)
    {
        var settings = new HostApplicationBuilderSettings
        {
            EnvironmentName = environmentName,
            // Suppress default appsettings.json lookup — tests run from the bin directory.
            Args = Array.Empty<string>()
        };

        var builder = Host.CreateApplicationBuilder(settings);

        builder.Configuration.AddInMemoryCollection(configValues);

        builder.AddLastTechTestObservability();

        var host = builder.Build();
        // Return the root provider so callers can resolve services.
        // The host must stay alive while tests resolve services from the provider.
        return host.Services;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // U1 — TracerProvider registered when enabled (CA-RO1-2)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void U1_AddLastTechTestObservability_WhenEnabled_ShouldRegisterTracerProvider()
    {
        // Arrange — Observability enabled, non-Testing environment
        var config = new Dictionary<string, string?>
        {
            ["Observability:Enabled"] = "true",
            ["Observability:Exporter"] = "Console",
            ["Observability:ServiceName"] = "TestService"
        };

        // Act
        var services = BuildProviderWith("Development", config);

        // Assert — SDK registers TracerProvider as a singleton in the container
        var tracerProvider = services.GetService<TracerProvider>();
        tracerProvider.Should().NotBeNull(
            "AddOpenTelemetry().WithTracing() must register a TracerProvider when Enabled=true");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // U2 — MeterProvider registered when enabled (CA-RO1-4)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void U2_AddLastTechTestObservability_WhenEnabled_ShouldRegisterMeterProvider()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            ["Observability:Enabled"] = "true",
            ["Observability:Exporter"] = "Console",
            ["Observability:ServiceName"] = "TestService"
        };

        // Act
        var services = BuildProviderWith("Development", config);

        // Assert — SDK registers MeterProvider as a singleton in the container
        var meterProvider = services.GetService<MeterProvider>();
        meterProvider.Should().NotBeNull(
            "AddOpenTelemetry().WithMetrics() must register a MeterProvider when Enabled=true");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // U3 — No TracerProvider when disabled (CA-RO1-5)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void U3_AddLastTechTestObservability_WhenEnabledFalse_ShouldNotRegisterActiveTracerProvider()
    {
        // Arrange — toggle explicitly off
        var config = new Dictionary<string, string?>
        {
            ["Observability:Enabled"] = "false"
        };

        // Act
        var services = BuildProviderWith("Production", config);

        // Assert — early return means no TracerProvider in the container
        var tracerProvider = services.GetService<TracerProvider>();
        tracerProvider.Should().BeNull(
            "no TracerProvider should be registered when Observability:Enabled=false");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // U4 — Defence-in-depth: Testing environment suppresses registration
    //      even when Enabled=true (CA-RO1-5)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void U4_AddLastTechTestObservability_WhenEnvironmentIsTesting_ShouldNotRegisterActiveTracerProvider()
    {
        // Arrange — Enabled=true but environment = "Testing"
        var config = new Dictionary<string, string?>
        {
            ["Observability:Enabled"] = "true",
            ["Observability:Exporter"] = "Console"
        };

        // Act
        var services = BuildProviderWith("Testing", config);

        // Assert — environment guard fires before AddOpenTelemetry() is called
        var tracerProvider = services.GetService<TracerProvider>();
        tracerProvider.Should().BeNull(
            "the Testing environment guard must prevent TracerProvider registration " +
            "regardless of the Enabled flag, protecting CI from OTEL overhead");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // U5 — ObservabilityOptions safe defaults (CA-RO1-5)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void U5_ObservabilityOptions_DefaultValues_ShouldHaveEnabledFalse()
    {
        // Arrange — options constructed with no external configuration
        var options = new ObservabilityOptions();

        // Assert — every default must be the "safe" value
        options.Enabled.Should().BeFalse(
            "observability is opt-in; an unconfigured instance must default to disabled");

        options.ServiceName.Should().Be("LastTechTest.API",
            "the default service name identifies this project in any OTEL backend");

        options.Exporter.Should().Be(ObservabilityExporter.Console,
            "Console is the zero-infrastructure exporter suitable for local development");

        options.OtlpEndpoint.Should().Be("http://localhost:4317",
            "the default OTLP endpoint points to a local collector/Jaeger instance");
    }
}
