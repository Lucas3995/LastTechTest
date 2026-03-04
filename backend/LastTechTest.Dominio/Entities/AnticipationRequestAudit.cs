namespace LastTechTest.Dominio.Entities;

/// <summary>RA-3: Audit record for anticipation request state transitions.</summary>
public class AnticipationRequestAudit
{
    public long Id { get; set; }
    public Guid RequestId { get; set; }
    public string Action { get; set; } = string.Empty; // Approve, Reject, Cancel
    public Guid? UserId { get; set; }
    public DateTime AtUtc { get; set; }
    public string? ReasonOrObservation { get; set; }
}