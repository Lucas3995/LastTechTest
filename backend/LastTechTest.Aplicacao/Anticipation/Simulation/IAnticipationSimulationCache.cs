namespace LastTechTest.Aplicacao.Anticipation.Simulation;

/// <summary>RA-4: Abstraction for simulation cache. Only last simulation per creator; TTL 2h real; exposed validity 20 min before real expiry.</summary>
public interface IAnticipationSimulationCache
{
    /// <summary>Stores simulation for creator (replaces any previous). Returns simulation code. TTL is real expiry duration (e.g. 2h).</summary>
    Task<string> StoreAsync(Guid creatorId, SimulationData data, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>Gets cached simulation by code, or null if not found or expired. Returns the entry (with IsUsed true) when the simulation was already converted, so the caller can return 422.</summary>
    Task<CachedSimulationEntry?> GetAsync(string simulationCode, CancellationToken cancellationToken = default);

    /// <summary>Marks the simulation as used so it cannot be converted again.</summary>
    Task MarkAsUsedAsync(string simulationCode, CancellationToken cancellationToken = default);
}