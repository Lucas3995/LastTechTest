namespace LastTechTest.Dominio.Interfaces;

/// <summary>RA-3: Records audit trail for anticipation request state transitions (approve, reject, cancel).</summary>
public interface IAnticipationAuditService
{
    Task RecordTransitionAsync(
        Guid requestId,
        string action,
        Guid? userId,
        DateTime atUtc,
        string? reasonOrObservation,
        CancellationToken cancellationToken = default);
}
