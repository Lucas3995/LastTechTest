using FluentValidation;

namespace LastTechTest.Aplicacao.Authentication.Commands.AdminResetUserPassword;

public sealed class AdminResetUserPasswordCommandValidator : AbstractValidator<AdminResetUserPasswordCommand>
{
    public AdminResetUserPasswordCommandValidator()
    {
        RuleFor(x => x)
            .Must(c => ExactlyOneIdentifierProvided(c))
            .WithMessage("Either Email or Id must be provided, but not both.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Id)
            .NotEmpty()
            .When(x => x.Id.HasValue);
    }

    private static bool ExactlyOneIdentifierProvided(AdminResetUserPasswordCommand c)
    {
        var hasEmail = !string.IsNullOrWhiteSpace(c.Email);
        var hasId = c.Id.HasValue && c.Id.Value != Guid.Empty;
        return (hasEmail || hasId) && (hasEmail != hasId);
    }
}
