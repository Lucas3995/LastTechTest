using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace LastTechTest.Persistencia.Repositories;

public sealed class AnticipationRequestRepository : IAnticipationRequestRepository
{
    private readonly ApplicationDbContext _context;

    public AnticipationRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AnticipationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AnticipationRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(AnticipationRequest request, CancellationToken cancellationToken = default)
    {
        await _context.AnticipationRequests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}