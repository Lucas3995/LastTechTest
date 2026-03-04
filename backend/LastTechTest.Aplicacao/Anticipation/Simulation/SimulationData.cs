namespace LastTechTest.Aplicacao.Anticipation.Simulation;

/// <summary>RA-4: Data stored in simulation cache for conversion to real request. Values must be identical when converting.</summary>
public sealed record SimulationData(
    Guid CreatorId,
    decimal RequestedAmount,
    decimal GrossAmount,
    decimal FeesAmount,
    decimal NetAmount,
    DateTime RequestedAtUtc);