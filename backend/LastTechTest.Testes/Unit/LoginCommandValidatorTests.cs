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

    [Theory]
    [InlineData("", "pwd")]
    [InlineData("invalid", "pwd")]
    public void Invalid_Email_Fails(string email, string pwd)
    {
        var cmd = new LoginCommand(email, pwd);
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