using FluentAssertions;

using LastTechTest.Aplicacao.Authentication.Commands.Logout;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class LogoutCommandValidatorTests
{
    private readonly LogoutCommandValidator _sut = new();

    [Fact]
    public void Valid_RefreshToken_Passes()
    {
        var cmd = new LogoutCommand("refresh-token-value");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_RefreshToken_Fails()
    {
        var cmd = new LogoutCommand("");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }
}