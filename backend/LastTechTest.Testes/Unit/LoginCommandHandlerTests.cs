using FluentAssertions;

using LastTechTest.Aplicacao.Authentication.Commands.Login;
using LastTechTest.Aplicacao.Common.Responses;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Infrastrutura;

using MediatR;

using Microsoft.Extensions.Configuration;

using Moq;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUserTokenRepository> _tokenRepo = new();
    private readonly IUserPasswordHasher _passwordHasher = new PasswordHasher();
    private readonly ITokenService _tokenService;
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
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
        _sut = new LoginCommandHandler(_userRepo.Object, _passwordHasher, _tokenService, _tokenRepo.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokens()
    {
        var user = new User();
        user.SetEmail("a@b.com");
        user.SetPasswordHash(_passwordHasher.HashPassword(user, "password123"));
        _userRepo.Setup(x => x.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepo.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _tokenRepo.Setup(x => x.AddAsync(It.IsAny<UserToken>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.Handle(new LoginCommand("a@b.com", "password123"), CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_UserNotFound_Throws()
    {
        _userRepo.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var act = () => _sut.Handle(new LoginCommand("a@b.com", "pwd"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Invalid credentials*");
    }

    [Fact]
    public async Task Handle_WrongPassword_Throws()
    {
        var user = new User();
        user.SetEmail("a@b.com");
        user.SetPasswordHash(_passwordHasher.HashPassword(user, "correct"));
        _userRepo.Setup(x => x.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var act = () => _sut.Handle(new LoginCommand("a@b.com", "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Invalid credentials*");
    }
}