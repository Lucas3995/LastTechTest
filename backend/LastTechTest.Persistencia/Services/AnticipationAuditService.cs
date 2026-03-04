using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

namespace LastTechTest.Persistencia.Services;

public sealed class AnticipationAuditService : IAnticipationAuditService
{
    private readonly ApplicationDbContext _context;

    public AnticipationAuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RecordTransitionAsync(
        Guid requestId,
        string action,
        Guid? userId,
        DateTime atUtc,
        string? reasonOrObservation,
        CancellationToken cancellationToken = default)
    {
        var audit = new AnticipationRequestAudit
        {
            RequestId = requestId,
            Action = action,
            UserId = userId,
            AtUtc = atUtc,
            ReasonOrObservation = reasonOrObservation
        };
        await _context.AnticipationRequestAudits.AddAsync(audit, cancellationToken);
        // Caller (handler) calls repository.SaveChangesAsync() to persist request + audit in one transaction.
    }
}
