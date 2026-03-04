using LastTechTest.Dominio.ValueObjects;

namespace LastTechTest.Dominio.Interfaces;

public interface IAnticipationCalculationService
{
    AnticipationCalculationResult? Calculate(decimal requestedAmount, IReadOnlyList<ReceivableInfo> receivables);
    (bool IsValid, string? ErrorMessage) ValidateWithinCreatorLimit(Guid creatorId, decimal requestedAmount);
}
