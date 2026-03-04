using FluentAssertions;

using LastTechTest.Dominio.Entities;
using LastTechTest.Infrastrutura;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class MfaServiceTests
{
    private readonly KeyGenerator _keyGen = new();
    private readonly MfaService _sut;

    public MfaServiceTests()
    {
        _sut = new MfaService(_keyGen);
    }

    [Fact]
    public async Task GenerateSecretAsync_ReturnsNonEmptySecret()
    {
        var user = new User();
        user.SetEmail("u@t.com");

        var secret = await _sut.GenerateSecretAsync(user);

        secret.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task VerifyCodeAsync_ValidCode_ReturnsTrue()
    {
        var user = new User();
        user.SetEmail("u@t.com");

        var result = await _sut.VerifyCodeAsync(user, "123456");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateSecretAsync_NullUser_Throws()
    {
        var act = async () => await _sut.GenerateSecretAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task VerifyCodeAsync_EmptyCode_Throws()
    {
        var user = new User();
        user.SetEmail("u@t.com");

        var act = async () => await _sut.VerifyCodeAsync(user, "");

        await act.Should().ThrowAsync<ArgumentException>();
    }
}