using FluentAssertions;

using LastTechTest.Dominio.Entities;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class UserEntityTests
{
    [Fact]
    public void New_User_Should_Be_Active()
    {
        var user = new User();

        user.Status.Should().Be(UserStatus.Active);
        user.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}