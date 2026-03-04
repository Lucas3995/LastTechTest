using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Enums;
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

    public async Task<AnticipationRequest?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AnticipationRequests
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(AnticipationRequest request, CancellationToken cancellationToken = default)
    {
        await _context.AnticipationRequests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>EF Core cannot translate AnticipationTransitionRules.IsAnalysisPendingStatus to SQL; use inline status check (must match domain: Created, Pending).</summary>
    public async Task<bool> HasPendingByCreatorAsync(Guid creatorId, CancellationToken cancellationToken = default)
    {
        return await _context.AnticipationRequests
            .AsNoTracking()
            .AnyAsync(x => x.CreatorId == creatorId && (x.Status == AnticipationRequestStatus.Created || x.Status == AnticipationRequestStatus.Pending), cancellationToken);
    }

    public async Task<(IReadOnlyList<AnticipationRequest> Items, int TotalCount)> ListAsync(
        Guid? creatorId,
        AnticipationRequestStatus? status,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var p = Math.Max(1, page);
        var size = Math.Clamp(pageSize, 1, 100);

        var query = _context.AnticipationRequests.AsNoTracking();

        if (creatorId.HasValue)
            query = query.Where(x => x.CreatorId == creatorId.Value);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);
        if (fromUtc.HasValue)
            query = query.Where(x => x.RequestedAtUtc >= fromUtc.Value);
        if (toUtc.HasValue)
            query = query.Where(x => x.RequestedAtUtc <= toUtc.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((p - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}