using System.IdentityModel.Tokens.Jwt;

using FluentAssertions;

using LastTechTest.Dominio.Entities;

using LastTechTest.Infrastrutura;

using Microsoft.Extensions.Configuration;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class TokenServiceTests
{
    [Fact]
    public void GenerateTokens_Should_Create_Access_And_Refresh_Tokens()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "unit-test-secret-key-should-be-long-enough",
            ["Jwt:Issuer"] = "UnitTests",
            ["Jwt:Audience"] = "UnitTests-Audience"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var keyGenerator = new KeyGenerator();
        var service = new TokenService(configuration, keyGenerator);
        var user = new User();

        var tokens = service.GenerateTokens(user);

        tokens.AccessToken.Should().NotBeNullOrWhiteSpace();
        tokens.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokens.AccessToken);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
    }

    [Fact]
    public void GetUserIdFromExpiredAccessToken_InvalidToken_ReturnsNull()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "unit-test-secret-key-should-be-long-enough",
            ["Jwt:Issuer"] = "UnitTests",
            ["Jwt:Audience"] = "UnitTests-Audience"
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();
        var service = new TokenService(configuration, new KeyGenerator());

        var userId = service.GetUserIdFromExpiredAccessToken("invalid.jwt.here");

        userId.Should().BeNull();
    }
}