using LastTechTest.Dominio.Enums;

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

    /// <summary>RA-3: Applies approval transition. Call only after validating with AnticipationTransitionRules.</summary>
    public void Approve()
    {
        Status = AnticipationRequestStatus.Approved;
    }

    /// <summary>RA-3: Applies rejection transition. Call only after validating with AnticipationTransitionRules.</summary>
    public void Reject()
    {
        Status = AnticipationRequestStatus.Rejected;
    }

    /// <summary>RA-3: Applies cancel-by-creator transition. Call only after validating with AnticipationTransitionRules.</summary>
    public void Cancel()
    {
        Status = AnticipationRequestStatus.CanceledByCreator;
    }

    /// <summary>For integration tests only: set status to simulate an existing state. Do not use in production code.</summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal void SetStatusForTest(AnticipationRequestStatus status)
    {
        Status = status;
    }
}