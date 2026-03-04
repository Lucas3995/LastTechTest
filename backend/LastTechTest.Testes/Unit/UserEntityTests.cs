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

    [Fact]
    public void SetEmail_And_Deactivate_Update_State()
    {
        var user = new User();
        user.SetEmail("a@b.com");
        user.SetPasswordHash("hash");
        user.MarkLoggedIn(DateTime.UtcNow);
        user.Deactivate();

        user.Email.Should().Be("a@b.com");
        user.PasswordHash.Should().Be("hash");
        user.LastLoginAtUtc.Should().NotBeNull();
        user.Status.Should().Be(UserStatus.Inactive);
    }
}