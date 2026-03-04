using FluentAssertions;

using LastTechTest.Dominio.Entities;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class UserTokenEntityTests
{
    [Fact]
    public void Initialize_Sets_Properties()
    {
        var userId = Guid.NewGuid();
        var expires = DateTime.UtcNow.AddDays(1);
        var token = new UserToken();

        token.Initialize(userId, "refresh-val", TokenType.Refresh, expires);

        token.UserId.Should().Be(userId);
        token.Value.Should().Be("refresh-val");
        token.Type.Should().Be(TokenType.Refresh);
        token.ExpiresAtUtc.Should().Be(expires);
    }

    [Fact]
    public void IsActiveAt_BeforeExpiry_AndNotRevoked_ReturnsTrue()
    {
        var token = new UserToken();
        token.Initialize(Guid.NewGuid(), "v", TokenType.Refresh, DateTime.UtcNow.AddHours(1));

        token.IsActiveAt(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void Revoke_SetsRevoked_IsActiveAtReturnsFalse()
    {
        var token = new UserToken();
        token.Initialize(Guid.NewGuid(), "v", TokenType.Refresh, DateTime.UtcNow.AddHours(1));
        token.Revoke();

        token.Revoked.Should().BeTrue();
        token.IsActiveAt(DateTime.UtcNow).Should().BeFalse();
    }
}