using LastTechTest.Dominio.Interfaces;

namespace LastTechTest.Aplicacao.Common.Services;

/// <summary>No-op implementation of audit service. Does not persist anything; for use until a real audit implementation is added.</summary>
public sealed class NoOpAnticipationAuditService : IAnticipationAuditService
{
    public Task RecordTransitionAsync(
        Guid requestId,
        string action,
        Guid? userId,
        DateTime atUtc,
        string? reasonOrObservation,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
