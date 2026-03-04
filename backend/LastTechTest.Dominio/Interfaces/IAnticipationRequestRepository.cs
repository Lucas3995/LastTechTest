using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Enums;

namespace LastTechTest.Dominio.Interfaces;

public interface IAnticipationRequestRepository
{
    Task<AnticipationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(AnticipationRequest request, CancellationToken cancellationToken = default);

    /// <summary>Listagem paginada com filtros (creator, status, período). Para uso por ListAnticipationRequestsQueryHandler.</summary>
    Task<(IReadOnlyList<AnticipationRequest> Items, int TotalCount)> ListAsync(
        Guid? creatorId,
        AnticipationRequestStatus? status,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}