using FluentAssertions;

using LastTechTest.Aplicacao.Authentication.Commands.Logout;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using MediatR;

using Moq;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class LogoutCommandHandlerTests
{
    private readonly Mock<IUserTokenRepository> _tokenRepo = new();
    private readonly LogoutCommandHandler _sut;

    public LogoutCommandHandlerTests()
    {
        _sut = new LogoutCommandHandler(_tokenRepo.Object);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_RevokesToken()
    {
        var token = new UserToken();
        token.Initialize(Guid.NewGuid(), "refresh-value", TokenType.Refresh, DateTime.UtcNow.AddDays(1));
        _tokenRepo.Setup(x => x.GetByValueAsync("refresh-value", It.IsAny<CancellationToken>())).ReturnsAsync(token);
        _tokenRepo.Setup(x => x.UpdateAsync(It.IsAny<UserToken>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.Handle(new LogoutCommand("refresh-value"), CancellationToken.None);

        result.Should().Be(Unit.Value);
        _tokenRepo.Verify(x => x.UpdateAsync(It.Is<UserToken>(t => t.Revoked), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TokenNotFound_ReturnsUnit()
    {
        _tokenRepo.Setup(x => x.GetByValueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserToken?)null);

        var result = await _sut.Handle(new LogoutCommand("missing"), CancellationToken.None);

        result.Should().Be(Unit.Value);
        _tokenRepo.Verify(x => x.UpdateAsync(It.IsAny<UserToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}