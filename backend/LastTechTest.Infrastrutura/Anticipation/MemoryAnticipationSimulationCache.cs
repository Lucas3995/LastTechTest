using System.Collections.Concurrent;

using LastTechTest.Aplicacao.Anticipation.Simulation;

namespace LastTechTest.Infrastrutura.Anticipation;

/// <summary>RA-4: In-memory implementation of IAnticipationSimulationCache. One entry per creator (replaced on new Store). TTL and IsUsed supported. Periodic eviction prevents unbounded memory growth.</summary>
public sealed class MemoryAnticipationSimulationCache : IAnticipationSimulationCache
{
    private const int EvictionIntervalSeconds = 300;

    private readonly ConcurrentDictionary<Guid, (string Code, CachedSimulationEntry Entry)> _byCreator = new();
    private readonly ConcurrentDictionary<string, Guid> _codeToCreator = new();
    private DateTime _lastEviction = DateTime.UtcNow;

    public Task<string> StoreAsync(Guid creatorId, SimulationData data, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        EvictExpiredIfDue();

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

    private void EvictExpiredIfDue()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastEviction).TotalSeconds < EvictionIntervalSeconds)
            return;

        _lastEviction = now;
        foreach (var kvp in _byCreator)
        {
            if (now > kvp.Value.Entry.RealExpiresAtUtc)
            {
                _codeToCreator.TryRemove(kvp.Value.Code, out _);
                _byCreator.TryRemove(kvp.Key, out _);
            }
        }
    }
}