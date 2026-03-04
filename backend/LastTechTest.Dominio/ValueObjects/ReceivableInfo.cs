namespace LastTechTest.Dominio.ValueObjects;

public sealed record ReceivableInfo(Guid Id, decimal Amount, DateTime DueDate, ReceivableStatus Status);

public enum ReceivableStatus
{
    Pending = 0,
    Eligible = 1,
    Paid = 2,
    Ineligible = 3
}
