using System.Collections.Concurrent;

using LastTechTest.Aplicacao.Anticipation.Simulation;

namespace LastTechTest.Infrastrutura.Anticipation;

/// <summary>RA-4: In-memory implementation of IAnticipationSimulationCache. One entry per creator (replaced on new Store). TTL and IsUsed supported. Ready for replacement with distributed cache (Redis, etc.) via same interface.</summary>
public sealed class MemoryAnticipationSimulationCache : IAnticipationSimulationCache
{
    private readonly ConcurrentDictionary<Guid, (string Code, CachedSimulationEntry Entry)> _byCreator = new();
    private readonly ConcurrentDictionary<string, Guid> _codeToCreator = new();

    public Task<string> StoreAsync(Guid creatorId, SimulationData data, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var code = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var realExpiry = DateTime.UtcNow.Add(ttl);
        var entry = new CachedSimulationEntry(data, realExpiry, IsUsed: false);

        if (_byCreator.TryGetValue(creatorId, out var old))
            _codeToCreator.TryRemove(old.Code, out _);
        _byCreator[creatorId] = (code, entry);
        _codeToCreator[code] = creatorId;
        return Task.FromResult(code);
    }

    public Task<CachedSimulationEntry?> GetAsync(string simulationCode, CancellationToken cancellationToken = default)
    {
        if (!_codeToCreator.TryGetValue(simulationCode, out var creatorId))
            return Task.FromResult<CachedSimulationEntry?>(null);
        if (!_byCreator.TryGetValue(creatorId, out var pair) || pair.Code != simulationCode)
            return Task.FromResult<CachedSimulationEntry?>(null);
        if (DateTime.UtcNow > pair.Entry.RealExpiresAtUtc)
            return Task.FromResult<CachedSimulationEntry?>(null);
        return Task.FromResult<CachedSimulationEntry?>(pair.Entry);
    }

    public Task MarkAsUsedAsync(string simulationCode, CancellationToken cancellationToken = default)
    {
        if (!_codeToCreator.TryGetValue(simulationCode, out var creatorId))
            return Task.CompletedTask;
        if (_byCreator.TryGetValue(creatorId, out var pair) && pair.Code == simulationCode)
        {
            var usedEntry = new CachedSimulationEntry(pair.Entry.Data, pair.Entry.RealExpiresAtUtc, IsUsed: true);
            _byCreator[creatorId] = (simulationCode, usedEntry);
        }
        return Task.CompletedTask;
    }
}