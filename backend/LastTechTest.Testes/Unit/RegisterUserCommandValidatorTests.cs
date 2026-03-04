using FluentAssertions;

using LastTechTest.Aplicacao.Authentication.Commands.RegisterUser;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _sut = new();

    [Fact]
    public void Valid_EmailAndLongPassword_Passes()
    {
        var cmd = new RegisterUserCommand("a@b.com", "password123");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShortPassword_Fails()
    {
        var cmd = new RegisterUserCommand("a@b.com", "short");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void InvalidEmail_Fails()
    {
        var cmd = new RegisterUserCommand("notanemail", "password123");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }
}