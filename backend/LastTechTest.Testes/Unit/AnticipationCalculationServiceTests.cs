using FluentAssertions;

using LastTechTest.Dominio.Services;
using LastTechTest.Dominio.ValueObjects;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class AnticipationCalculationServiceTests
{
    private readonly AnticipationCalculationService _sut = new();

    [Fact]
    public void U1_AnticipationCalculationService_ValidInputs_Should_ReturnGrossFeesAndNet()
    {
        var receivables = new List<ReceivableInfo>
        {
            new(Guid.NewGuid(), 5000m, DateTime.UtcNow.AddDays(10), ReceivableStatus.Eligible),
            new(Guid.NewGuid(), 5000m, DateTime.UtcNow.AddDays(20), ReceivableStatus.Eligible)
        };
        const decimal requestedAmount = 1000m;

        var result = _sut.Calculate(requestedAmount, receivables);

        result.Should().NotBeNull();
        result!.GrossAmount.Should().Be(requestedAmount);
        // Regra de negócio: taxa 5%; valor líquido = 95% do valor solicitado
        const decimal feeRate = 0.05m;
        const decimal netRate = 0.95m;
        result.FeesAmount.Should().Be(requestedAmount * feeRate, "taxa de adiantamento é 5%");
        result.NetAmount.Should().Be(requestedAmount * netRate, "valor adiantado ao usuário é 95% do solicitado");
        result.NetAmount.Should().Be(result.GrossAmount - result.FeesAmount);
    }

    [Fact]
    public void U2_AnticipationCalculationService_AboveCreatorLimit_Should_ThrowOrReturnError()
    {
        var creatorId = Guid.NewGuid();
        const decimal aboveLimit = 50_000m;

        var (isValid, errorMessage) = _sut.ValidateWithinCreatorLimit(creatorId, aboveLimit);

        isValid.Should().BeFalse();
        errorMessage.Should().NotBeNullOrWhiteSpace();
    }
}
