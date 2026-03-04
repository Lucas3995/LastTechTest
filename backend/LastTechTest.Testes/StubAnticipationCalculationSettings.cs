using LastTechTest.Dominio.Interfaces;

namespace LastTechTest.Testes;

/// <summary>Stub for tests: default fee rate 5% and creator limit 10_000.</summary>
public sealed class StubAnticipationCalculationSettings : IAnticipationCalculationSettings
{
    public decimal FeeRate => 0.05m;
    public decimal DefaultCreatorLimit => 10_000m;
}
