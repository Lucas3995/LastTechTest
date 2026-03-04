using FluentAssertions;

using LastTechTest.Dominio.Entities;
using LastTechTest.Infrastrutura;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void HashPassword_ValidInput_ReturnsNonEmptyHash()
    {
        var user = new User();
        user.SetEmail("a@b.com");

        var hash = _sut.HashPassword(user, "password123");

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public void HashPassword_SamePassword_DifferentSalts_ProduceDifferentHashes()
    {
        var user = new User();
        user.SetEmail("a@b.com");

        var hash1 = _sut.HashPassword(user, "password123");
        var hash2 = _sut.HashPassword(user, "password123");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyHashedPassword_CorrectPassword_ReturnsTrue()
    {
        var user = new User();
        user.SetEmail("a@b.com");
        var hash = _sut.HashPassword(user, "password123");

        var result = _sut.VerifyHashedPassword(user, hash, "password123");

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyHashedPassword_WrongPassword_ReturnsFalse()
    {
        var user = new User();
        user.SetEmail("a@b.com");
        var hash = _sut.HashPassword(user, "password123");

        var result = _sut.VerifyHashedPassword(user, hash, "wrong");

        result.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_NullUser_Throws()
    {
        var act = () => _sut.HashPassword(null!, "pwd");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HashPassword_EmptyPassword_Throws()
    {
        var user = new User();
        user.SetEmail("a@b.com");

        var act = () => _sut.HashPassword(user, "");

        act.Should().Throw<ArgumentException>();
    }
}