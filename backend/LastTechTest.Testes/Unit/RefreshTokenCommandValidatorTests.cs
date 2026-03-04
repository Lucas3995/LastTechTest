using FluentAssertions;

using LastTechTest.Aplicacao.Authentication.Commands.RefreshToken;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _sut = new();

    [Fact]
    public void Valid_RefreshToken_Passes()
    {
        var cmd = new RefreshTokenCommand("refresh-token-value");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_RefreshToken_Fails()
    {
        var cmd = new RefreshTokenCommand("");
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }
}