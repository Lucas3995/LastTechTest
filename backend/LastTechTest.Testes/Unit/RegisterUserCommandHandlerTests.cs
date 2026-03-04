using FluentAssertions;

using LastTechTest.Aplicacao.Authentication.Commands.RegisterUser;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Infrastrutura;

using Moq;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUserTokenRepository> _tokenRepo = new();
    private readonly IUserPasswordHasher _passwordHasher = new PasswordHasher();
    private readonly ITokenService _tokenService;
    private readonly RegisterUserCommandHandler _sut;

    public RegisterUserCommandHandlerTests()
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
        _sut = new RegisterUserCommandHandler(_userRepo.Object, _passwordHasher, _tokenService, _tokenRepo.Object);
    }

    [Fact]
    public async Task Handle_NewEmail_ReturnsTokens()
    {
        _userRepo.Setup(x => x.GetByEmailAsync("new@b.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _userRepo.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _tokenRepo.Setup(x => x.AddAsync(It.IsAny<UserToken>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.Handle(new RegisterUserCommand("new@b.com", "password123"), CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_Throws()
    {
        var existing = new User();
        existing.SetEmail("existing@b.com");
        _userRepo.Setup(x => x.GetByEmailAsync("existing@b.com", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var act = () => _sut.Handle(new RegisterUserCommand("existing@b.com", "password123"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    }
}