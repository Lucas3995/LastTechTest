using LastTechTest.Aplicacao.Anticipation.Simulation;

namespace LastTechTest.Infrastrutura.Anticipation;

/// <summary>RA-4: Stub implementation for DI until real in-memory cache is implemented. All methods throw.</summary>
public sealed class NotImplementedAnticipationSimulationCache : IAnticipationSimulationCache
{
    public Task<string> StoreAsync(Guid creatorId, SimulationData data, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("RA-4 IAnticipationSimulationCache.StoreAsync not implemented.");
    }

    public Task<CachedSimulationEntry?> GetAsync(string simulationCode, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("RA-4 IAnticipationSimulationCache.GetAsync not implemented.");
    }

    public Task MarkAsUsedAsync(string simulationCode, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("RA-4 IAnticipationSimulationCache.MarkAsUsedAsync not implemented.");
    }
}
