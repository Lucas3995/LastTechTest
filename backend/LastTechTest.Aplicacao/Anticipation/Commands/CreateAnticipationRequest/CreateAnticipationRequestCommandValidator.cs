using FluentValidation;

namespace LastTechTest.Aplicacao.Anticipation.Commands.CreateAnticipationRequest;

public sealed class CreateAnticipationRequestCommandValidator : AbstractValidator<CreateAnticipationRequestCommand>
{
    public CreateAnticipationRequestCommandValidator()
    {
        RuleFor(x => x.RequestedAmount)
            .GreaterThanOrEqualTo(100m)
            .WithMessage("Requested amount must be 100 or more.");
    }
}