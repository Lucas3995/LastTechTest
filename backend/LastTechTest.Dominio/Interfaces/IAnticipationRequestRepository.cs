using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Enums;
using LastTechTest.Dominio.ValueObjects;

namespace LastTechTest.Dominio.Interfaces;

public interface IAnticipationRequestRepository
{
    Task<AnticipationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets the entity tracked for update (no AsNoTracking). Use for transitions then SaveChangesAsync.</summary>
    Task<AnticipationRequest?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(AnticipationRequest request, CancellationToken cancellationToken = default);

    /// <summary>RC-1: Returns true if the creator has at least one request in analysis (Created or Pending).</summary>
    Task<bool> HasPendingByCreatorAsync(Guid creatorId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Listagem paginada com filtros (creator, status, período). Para uso por ListAnticipationRequestsQueryHandler.</summary>
    Task<(IReadOnlyList<AnticipationRequest> Items, int TotalCount)> ListAsync(
        ListAnticipationRequestsFilter filter,
        CancellationToken cancellationToken = default);
}