using FluentAssertions;

using LastTechTest.Aplicacao.Authentication.Commands.AdminResetUserPassword;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class AdminResetUserPasswordCommandValidatorTests
{
    private readonly AdminResetUserPasswordCommandValidator _sut = new();

    [Fact]
    public void Valid_EmailOnly_Passes()
    {
        var cmd = new AdminResetUserPasswordCommand("user@example.com", null);
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valid_IdOnly_Passes()
    {
        var cmd = new AdminResetUserPasswordCommand(null, Guid.NewGuid());
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void BothEmpty_Fails()
    {
        var cmd = new AdminResetUserPasswordCommand(null, null);
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Either Email or Id must be provided"));
    }

    [Fact]
    public void BothFilled_Fails()
    {
        var cmd = new AdminResetUserPasswordCommand("a@b.com", Guid.NewGuid());
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Either Email or Id must be provided"));
    }

    [Fact]
    public void InvalidEmail_WhenEmailProvided_Fails()
    {
        var cmd = new AdminResetUserPasswordCommand("not-an-email", null);
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void EmptyGuid_WhenIdProvided_Fails()
    {
        var cmd = new AdminResetUserPasswordCommand(null, Guid.Empty);
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void WhitespaceEmail_WithNoId_Fails()
    {
        var cmd = new AdminResetUserPasswordCommand("   ", null);
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }
}
