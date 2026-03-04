using LastTechTest.Dominio.ValueObjects;

namespace LastTechTest.Dominio.Interfaces;

public interface IReceivableRepository
{
    Task<IReadOnlyList<ReceivableInfo>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>Obtém recebíveis elegíveis do creator quando o cliente não informa ReceivableIds (apenas os 3 campos).</summary>
    Task<IReadOnlyList<ReceivableInfo>> GetEligibleByCreatorIdAsync(Guid creatorId, CancellationToken cancellationToken = default);
}