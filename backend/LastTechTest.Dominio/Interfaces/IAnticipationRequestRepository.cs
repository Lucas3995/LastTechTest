using LastTechTest.Dominio.Entities;

namespace LastTechTest.Dominio.Interfaces;

public interface IAnticipationRequestRepository
{
    Task<AnticipationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(AnticipationRequest request, CancellationToken cancellationToken = default);
}