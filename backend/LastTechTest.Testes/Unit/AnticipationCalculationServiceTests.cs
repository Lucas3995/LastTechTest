using FluentAssertions;

using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.Services;
using LastTechTest.Dominio.ValueObjects;

using Moq;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class AnticipationCalculationServiceTests
{
    private static IAnticipationCalculationSettings CreateDefaultSettings()
    {
        var mock = new Mock<IAnticipationCalculationSettings>();
        mock.Setup(s => s.FeeRate).Returns(0.05m);
        mock.Setup(s => s.DefaultCreatorLimit).Returns(10_000m);
        return mock.Object;
    }

    private readonly AnticipationCalculationService _sut = new(CreateDefaultSettings());

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

    [Fact]
    public void ValidateWithinCreatorLimit_WithinLimit_Should_ReturnValid()
    {
        var (isValid, errorMessage) = _sut.ValidateWithinCreatorLimit(Guid.NewGuid(), 5_000m);

        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Fact]
    public void ValidateWithinCreatorLimit_ZeroAmount_Should_ReturnInvalid()
    {
        var (isValid, errorMessage) = _sut.ValidateWithinCreatorLimit(Guid.NewGuid(), 0m);

        isValid.Should().BeFalse();
        errorMessage.Should().Contain("positive");
    }

    [Fact]
    public void Calculate_EmptyReceivables_ReturnsNull()
    {
        var result = _sut.Calculate(100m, new List<ReceivableInfo>());

        result.Should().BeNull();
    }

    [Fact]
    public void Calculate_RequestedAmountExceedsEligibleTotal_ReturnsNull()
    {
        var receivables = new List<ReceivableInfo>
        {
            new(Guid.NewGuid(), 100m, DateTime.UtcNow.AddDays(10), ReceivableStatus.Eligible)
        };

        var result = _sut.Calculate(500m, receivables);

        result.Should().BeNull();
    }

    [Fact]
    public void Calculate_RequestedAmountZero_ReturnsNull()
    {
        var receivables = new List<ReceivableInfo>
        {
            new(Guid.NewGuid(), 1000m, DateTime.UtcNow.AddDays(10), ReceivableStatus.Eligible)
        };

        var result = _sut.Calculate(0m, receivables);

        result.Should().BeNull();
    }

    [Fact]
    public void Calculate_Uses_Injected_FeeRate()
    {
        var mock = new Mock<IAnticipationCalculationSettings>();
        mock.Setup(s => s.FeeRate).Returns(0.10m);
        mock.Setup(s => s.DefaultCreatorLimit).Returns(10_000m);
        var sut = new AnticipationCalculationService(mock.Object);
        var receivables = new List<ReceivableInfo>
        {
            new(Guid.NewGuid(), 5000m, DateTime.UtcNow.AddDays(10), ReceivableStatus.Eligible)
        };

        var result = sut.Calculate(1000m, receivables);

        result.Should().NotBeNull();
        result!.FeesAmount.Should().Be(100m, "10% of 1000");
        result.NetAmount.Should().Be(900m);
    }

    [Fact]
    public void ValidateWithinCreatorLimit_Uses_Injected_Limit()
    {
        var mock = new Mock<IAnticipationCalculationSettings>();
        mock.Setup(s => s.FeeRate).Returns(0.05m);
        mock.Setup(s => s.DefaultCreatorLimit).Returns(5_000m);
        var sut = new AnticipationCalculationService(mock.Object);

        var (isValid, errorMessage) = sut.ValidateWithinCreatorLimit(Guid.NewGuid(), 6_000m);

        isValid.Should().BeFalse();
        errorMessage.Should().Contain("5000");
    }
}