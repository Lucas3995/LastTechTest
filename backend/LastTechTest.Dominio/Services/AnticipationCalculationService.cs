using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.ValueObjects;

namespace LastTechTest.Dominio.Services;

public sealed class AnticipationCalculationService : IAnticipationCalculationService
{
    private readonly IAnticipationCalculationSettings _settings;

    public AnticipationCalculationService(IAnticipationCalculationSettings settings)
    {
        _settings = settings;
    }

    public AnticipationCalculationResult? Calculate(decimal requestedAmount, IReadOnlyList<ReceivableInfo> receivables)
    {
        if (receivables.Count == 0) return null;
        var eligibleTotal = receivables.Where(r => r.Status == ReceivableStatus.Eligible).Sum(r => r.Amount);
        if (requestedAmount <= 0 || requestedAmount > eligibleTotal) return null;
        var feeRate = _settings.FeeRate;
        var netRate = 1m - feeRate;
        var fees = requestedAmount * feeRate;
        var net = requestedAmount * netRate;
        return new AnticipationCalculationResult(requestedAmount, fees, net);
    }

    public (bool IsValid, string? ErrorMessage) ValidateWithinCreatorLimit(Guid creatorId, decimal requestedAmount)
    {
        if (requestedAmount <= 0)
            return (false, "Requested amount must be positive.");
        var limit = _settings.DefaultCreatorLimit;
        if (requestedAmount > limit)
            return (false, $"Requested amount exceeds creator limit ({limit}).");
        return (true, null);
    }
}