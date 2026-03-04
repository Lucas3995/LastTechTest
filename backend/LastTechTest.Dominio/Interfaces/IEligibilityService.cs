using LastTechTest.Dominio.ValueObjects;

namespace LastTechTest.Dominio.Interfaces;

public interface IEligibilityService
{
    bool IsEligible(ReceivableInfo receivable);
    IReadOnlyList<ReceivableInfo> FilterEligible(IEnumerable<ReceivableInfo> receivables);
}