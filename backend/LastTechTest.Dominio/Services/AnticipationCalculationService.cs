using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.ValueObjects;

namespace LastTechTest.Dominio.Services;

public sealed class AnticipationCalculationService : IAnticipationCalculationService
{
    public AnticipationCalculationResult? Calculate(decimal requestedAmount, IReadOnlyList<ReceivableInfo> receivables)
    {
        if (receivables.Count == 0) return null;
        var eligibleTotal = receivables.Where(r => r.Status == ReceivableStatus.Eligible).Sum(r => r.Amount);
        if (requestedAmount <= 0 || requestedAmount > eligibleTotal) return null;
        // Regra: taxa 5%; valor adiantado ao usuário = 95% do valor solicitado
        const decimal feeRate = 0.05m;
        const decimal netRate = 0.95m;
        var fees = requestedAmount * feeRate;
        var net = requestedAmount * netRate;
        return new AnticipationCalculationResult(requestedAmount, fees, net);
    }

    public (bool IsValid, string? ErrorMessage) ValidateWithinCreatorLimit(Guid creatorId, decimal requestedAmount)
    {
        if (requestedAmount <= 0)
            return (false, "Requested amount must be positive.");
        // Placeholder: real limit will come from InstrucoesProjeto / demandas
        const decimal placeholderLimit = 10_000m;
        if (requestedAmount > placeholderLimit)
            return (false, $"Requested amount exceeds creator limit ({placeholderLimit}).");
        return (true, null);
    }
}