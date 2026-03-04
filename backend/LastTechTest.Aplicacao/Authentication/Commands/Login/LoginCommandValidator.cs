using FluentValidation;

namespace LastTechTest.Aplicacao.Authentication.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>Accepts either a valid email or a login identifier (e.g. username without domain, like usu_acesso_total for admin).</summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email or login is required.");

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}