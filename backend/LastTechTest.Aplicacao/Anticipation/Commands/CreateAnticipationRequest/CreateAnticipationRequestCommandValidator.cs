using FluentValidation;

namespace LastTechTest.Aplicacao.Anticipation.Commands.CreateAnticipationRequest;

public sealed class CreateAnticipationRequestCommandValidator : AbstractValidator<CreateAnticipationRequestCommand>
{
    public CreateAnticipationRequestCommandValidator()
    {
        RuleFor(x => x.RequestedAmount)
            .GreaterThan(0)
            .WithMessage("Requested amount must be greater than zero.");
    }
}