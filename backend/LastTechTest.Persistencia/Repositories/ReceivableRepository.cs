using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.ValueObjects;

namespace LastTechTest.Persistencia.Repositories;

public sealed class ReceivableRepository : IReceivableRepository
{
    public Task<IReadOnlyList<ReceivableInfo>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var list = ids.Select(id => new ReceivableInfo(id, 10_000m, DateTime.UtcNow.AddDays(30), ReceivableStatus.Eligible)).ToList();
        return Task.FromResult<IReadOnlyList<ReceivableInfo>>(list);
    }

    public Task<IReadOnlyList<ReceivableInfo>> GetEligibleByCreatorIdAsync(Guid creatorId, CancellationToken cancellationToken = default)
    {
        _ = creatorId;
        _ = cancellationToken;
        var list = new List<ReceivableInfo>
        {
            new(Guid.NewGuid(), 10_000m, DateTime.UtcNow.AddDays(30), ReceivableStatus.Eligible)
        };
        return Task.FromResult<IReadOnlyList<ReceivableInfo>>(list);
    }
}