using FluentAssertions;

using LastTechTest.Aplicacao.Anticipation.Commands.CreateAnticipationRequest;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class CreateAnticipationRequestCommandValidatorTests
{
    private readonly CreateAnticipationRequestCommandValidator _validator = new();

    [Fact]
    public void U5_CreateAnticipationRequestCommandValidator_InvalidAmount_Should_FailValidation()
    {
        var command = new CreateAnticipationRequestCommand(0);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.RequestedAmount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void U5_CreateAnticipationRequestCommandValidator_NegativeOrZeroAmount_Should_FailValidation(decimal amount)
    {
        var command = new CreateAnticipationRequestCommand(amount);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    /// <summary>RC-1: Requested amount &lt; 100 must fail (rule is &gt;= 100).</summary>
    [Theory]
    [InlineData(99)]
    [InlineData(99.99)]
    public void U5b_CreateAnticipationRequestCommandValidator_AmountLessThan100_Should_FailValidation(decimal amount)
    {
        var command = new CreateAnticipationRequestCommand(amount);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.RequestedAmount));
    }

    /// <summary>RC-1: Requested amount &gt;= 100 must pass (100, 100.00, 100.01, 101).</summary>
    [Theory]
    [InlineData(100)]
    [InlineData(100.00)]
    [InlineData(100.01)]
    [InlineData(101)]
    public void U6_CreateAnticipationRequestCommandValidator_Amount100OrMore_Should_Pass(decimal amount)
    {
        var command = new CreateAnticipationRequestCommand(amount);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void U6_CreateAnticipationRequestCommandValidator_ValidCommand_Should_Pass()
    {
        var command = new CreateAnticipationRequestCommand(1000m);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}