using LastTechTest.Dominio.Enums;
using LastTechTest.Dominio.Services;

namespace LastTechTest.Dominio.Entities;

public class AnticipationRequest
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CreatorId { get; private set; }
    public AnticipationRequestStatus Status { get; private set; }

    private AnticipationRequest() { }

    public decimal RequestedAmount { get; private set; }
    public decimal GrossAmount { get; private set; }
    public decimal FeesAmount { get; private set; }
    public decimal NetAmount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    /// <summary>Data informada pelo cliente para a solicitação (data_solicitacao); quando omitida usa-se o momento da criação.</summary>
    public DateTime RequestedAtUtc { get; private set; }

    public static AnticipationRequest Create(Guid creatorId, decimal requestedAmount, decimal grossAmount, decimal feesAmount, decimal netAmount, DateTime? requestedAtUtc = null)
    {
        var at = requestedAtUtc ?? DateTime.UtcNow;
        return new AnticipationRequest
        {
            CreatorId = creatorId,
            Status = AnticipationRequestStatus.Pending,
            RequestedAmount = requestedAmount,
            GrossAmount = grossAmount,
            FeesAmount = feesAmount,
            NetAmount = netAmount,
            RequestedAtUtc = at
        };
    }

    public string Protocol => Id.ToString("N")[..8].ToUpperInvariant();

    /// <summary>RA-3: Applies approval transition with invariant validation.</summary>
    public void Approve(string? role)
    {
        if (!AnticipationTransitionRules.CanApprove(Status, role))
            throw new InvalidOperationException("Cannot approve: transition not allowed from current state or insufficient permissions.");
        Status = AnticipationRequestStatus.Approved;
    }

    /// <summary>RA-3: Applies rejection transition with invariant validation.</summary>
    public void Reject(string? role)
    {
        if (!AnticipationTransitionRules.CanReject(Status, role))
            throw new InvalidOperationException("Cannot reject: transition not allowed from current state or insufficient permissions.");
        Status = AnticipationRequestStatus.Rejected;
    }

    /// <summary>RA-3: Applies cancel-by-creator transition with invariant validation.</summary>
    public void Cancel(string? role, bool isOwner)
    {
        if (!AnticipationTransitionRules.CanCancel(Status, role, isOwner))
            throw new InvalidOperationException("Cannot cancel: transition not allowed from current state or insufficient permissions.");
        Status = AnticipationRequestStatus.CanceledByCreator;
    }

    /// <summary>For integration tests only: set status to simulate an existing state. Do not use in production code.</summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal void SetStatusForTest(AnticipationRequestStatus status)
    {
        Status = status;
    }
}