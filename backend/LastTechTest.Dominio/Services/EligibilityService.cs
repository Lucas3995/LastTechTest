using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.ValueObjects;

namespace LastTechTest.Dominio.Services;

public sealed class EligibilityService : IEligibilityService
{
    public bool IsEligible(ReceivableInfo receivable)
    {
        if (receivable.Status != ReceivableStatus.Eligible) return false;
        if (receivable.DueDate < DateTime.UtcNow.Date) return false;
        if (receivable.Amount <= 0) return false;
        return true;
    }

    public IReadOnlyList<ReceivableInfo> FilterEligible(IEnumerable<ReceivableInfo> receivables)
    {
        return receivables.Where(IsEligible).ToList();
    }
}
