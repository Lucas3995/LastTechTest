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

    [Fact]
    public void U6_CreateAnticipationRequestCommandValidator_ValidCommand_Should_Pass()
    {
        var command = new CreateAnticipationRequestCommand(1000m);
        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}