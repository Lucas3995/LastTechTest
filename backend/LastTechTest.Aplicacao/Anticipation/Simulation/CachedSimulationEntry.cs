namespace LastTechTest.Aplicacao.Anticipation.Simulation;

/// <summary>RA-4: Entry returned from cache: simulation data, real expiry (TTL 2h), and whether it was already used for conversion.</summary>
public sealed record CachedSimulationEntry(SimulationData Data, DateTime RealExpiresAtUtc, bool IsUsed);
