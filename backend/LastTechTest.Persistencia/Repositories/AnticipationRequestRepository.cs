using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Enums;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.Services;
using LastTechTest.Dominio.ValueObjects;

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

    /// <summary>Uses domain single source AnticipationTransitionRules.AnalysisPendingStatuses; EF translates Contains to SQL IN.</summary>
    public async Task<bool> HasPendingByCreatorAsync(Guid creatorId, CancellationToken cancellationToken = default)
    {
        var analysisPending = AnticipationTransitionRules.AnalysisPendingStatuses;
        return await _context.AnticipationRequests
            .AsNoTracking()
            .AnyAsync(x => x.CreatorId == creatorId && analysisPending.Contains(x.Status), cancellationToken);
    }

    public async Task<(IReadOnlyList<AnticipationRequest> Items, int TotalCount)> ListAsync(
        ListAnticipationRequestsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var p = Math.Max(1, filter.Page);
        var size = Math.Clamp(filter.PageSize, 1, 100);

        var query = _context.AnticipationRequests.AsNoTracking();

        if (filter.CreatorId.HasValue)
            query = query.Where(x => x.CreatorId == filter.CreatorId.Value);
        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status.Value);
        if (filter.FromUtc.HasValue)
            query = query.Where(x => x.RequestedAtUtc >= filter.FromUtc.Value);
        if (filter.ToUtc.HasValue)
            query = query.Where(x => x.RequestedAtUtc <= filter.ToUtc.Value);

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