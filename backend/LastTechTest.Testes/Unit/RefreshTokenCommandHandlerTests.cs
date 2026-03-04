using FluentAssertions;

using LastTechTest.Aplicacao.Authentication.Commands.RefreshToken;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Infrastrutura;

using Microsoft.Extensions.Configuration;

using Moq;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserTokenRepository> _tokenRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly ITokenService _tokenService;
    private readonly RefreshTokenCommandHandler _sut;

    public RefreshTokenCommandHandlerTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "unit-test-secret-key-should-be-long-enough",
                ["Jwt:Issuer"] = "UnitTests",
                ["Jwt:Audience"] = "UnitTests-Audience"
            }!)
            .Build();
        _tokenService = new TokenService(config, new KeyGenerator());
        _sut = new RefreshTokenCommandHandler(_tokenRepo.Object, _userRepo.Object, _tokenService);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewTokens()
    {
        var user = new User();
        user.SetEmail("u@t.com");
        var token = new UserToken();
        token.Initialize(user.Id, "old-refresh", TokenType.Refresh, DateTime.UtcNow.AddDays(1));
        _tokenRepo.Setup(x => x.GetByValueAsync("old-refresh", It.IsAny<CancellationToken>())).ReturnsAsync(token);
        _tokenRepo.Setup(x => x.UpdateAsync(It.IsAny<UserToken>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _tokenRepo.Setup(x => x.AddAsync(It.IsAny<UserToken>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _userRepo.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.Handle(new RefreshTokenCommand("old-refresh"), CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_InvalidRefreshToken_Throws()
    {
        _tokenRepo.Setup(x => x.GetByValueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserToken?)null);

        var act = () => _sut.Handle(new RefreshTokenCommand("invalid"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Invalid refresh token*");
    }
}