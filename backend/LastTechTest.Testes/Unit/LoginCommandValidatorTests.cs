using FluentAssertions;

using LastTechTest.Aplicacao.Authentication.Commands.Login;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _sut = new();

    [Fact]
    public void Valid_EmailAndPassword_Passes()
    {
        var cmd = new LoginCommand("a@b.com", "password");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valid_LoginNameAndPassword_Passes()
    {
        var cmd = new LoginCommand("usu_acesso_total", "password");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_Email_Fails()
    {
        var cmd = new LoginCommand("", "pwd");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_Password_Fails()
    {
        var cmd = new LoginCommand("a@b.com", "");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }
}