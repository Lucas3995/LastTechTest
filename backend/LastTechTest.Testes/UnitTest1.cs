using FluentAssertions;

using LastTechTest.Dominio.Entities;
using LastTechTest.Infrastrutura;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_And_Verify_Succeeds_For_Valid_Password()
    {
        var hasher = new PasswordHasher();
        var user = new User();

        var hash = hasher.HashPassword(user, "StrongPassword123!");

        hash.Should().NotBeNullOrWhiteSpace();
        hasher.VerifyHashedPassword(user, hash, "StrongPassword123!").Should().BeTrue();
    }

    [Fact]
    public void VerifyHashedPassword_Fails_For_Invalid_Password()
    {
        var hasher = new PasswordHasher();
        var user = new User();

        var hash = hasher.HashPassword(user, "StrongPassword123!");

        hasher.VerifyHashedPassword(user, hash, "WrongPassword").Should().BeFalse();
    }
}