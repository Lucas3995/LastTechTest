using FluentAssertions;

using LastTechTest.Aplicacao.Anticipation.Commands.SimulateAnticipationRequest;

using Xunit;

namespace LastTechTest.Testes.Unit;

/// <summary>RA-4: Same rule as RA-1 — RequestedAmount >= 100 (InstrucoesProjeto). Boundary: 99, 100, 100.01.</summary>
[Trait("Category", "Unit")]
public class SimulateAnticipationRequestCommandValidatorTests
{
    private readonly SimulateAnticipationRequestCommandValidator _sut = new();

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(99.99)]
    public void RequestedAmount_LessThan100_Should_BeInvalid(decimal amount)
    {
        var command = new SimulateAnticipationRequestCommand(amount, null, null);
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.RequestedAmount) && e.ErrorMessage != null && e.ErrorMessage.Contains("100"));
    }

    [Theory]
    [InlineData(100)]
    [InlineData(100.00)]
    [InlineData(100.01)]
    [InlineData(101)]
    public void RequestedAmount_100OrMore_Should_BeValid(decimal amount)
    {
        var command = new SimulateAnticipationRequestCommand(amount, null, null);
        var result = _sut.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}