using FluentAssertions;

using LastTechTest.Infrastrutura;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class KeyGeneratorTests
{
    private readonly KeyGenerator _sut = new();

    [Fact]
    public void GenerateSecureToken_ReturnsRequestedLength()
    {
        var token = _sut.GenerateSecureToken(32);

        token.Should().NotBeNullOrWhiteSpace();
        token.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerateSecureToken_DifferentCalls_ProduceDifferentValues()
    {
        var t1 = _sut.GenerateSecureToken(64);
        var t2 = _sut.GenerateSecureToken(64);

        t1.Should().NotBe(t2);
    }
}