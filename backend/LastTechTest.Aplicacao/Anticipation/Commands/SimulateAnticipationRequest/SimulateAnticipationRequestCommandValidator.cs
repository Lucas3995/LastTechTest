using FluentValidation;

namespace LastTechTest.Aplicacao.Anticipation.Commands.SimulateAnticipationRequest;

/// <summary>RA-4: Same rule as creation — requested amount >= 100 (InstrucoesProjeto).</summary>
public sealed class SimulateAnticipationRequestCommandValidator : AbstractValidator<SimulateAnticipationRequestCommand>
{
    public SimulateAnticipationRequestCommandValidator()
    {
        RuleFor(x => x.RequestedAmount)
            .GreaterThanOrEqualTo(100m)
            .WithMessage("Requested amount must be 100 or more.");
    }
}