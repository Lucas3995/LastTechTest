using FluentAssertions;

using LastTechTest.Aplicacao.Authentication.Commands.RegisterUser;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Infrastrutura;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

using Moq;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUserTokenRepository> _tokenRepo = new();
    private readonly IUserPasswordHasher _passwordHasher = new PasswordHasher();
    private readonly Mock<UserManager<IdentityUser<Guid>>> _userManager;
    private readonly ITokenService _tokenService;
    private readonly RegisterUserCommandHandler _sut;

    public RegisterUserCommandHandlerTests()
    {
        _userManager = CreateUserManagerMock();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "unit-test-secret-key-should-be-long-enough",
                ["Jwt:Issuer"] = "UnitTests",
                ["Jwt:Audience"] = "UnitTests-Audience"
            }!)
            .Build();
        _tokenService = new TokenService(config, new KeyGenerator());
        _sut = new RegisterUserCommandHandler(_userRepo.Object, _passwordHasher, _tokenService, _tokenRepo.Object, _userManager.Object);
    }

    [Fact]
    public async Task Handle_NewEmail_ReturnsTokens()
    {
        _userManager.Setup(x => x.FindByEmailAsync("new@b.com")).ReturnsAsync((IdentityUser<Guid>?)null);
        _userManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser<Guid>>(), "password123"))
            .ReturnsAsync(IdentityResult.Success);

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
        var existingIdentity = new IdentityUser<Guid> { Email = "existing@b.com" };
        _userManager.Setup(x => x.FindByEmailAsync("existing@b.com")).ReturnsAsync(existingIdentity);

        var act = () => _sut.Handle(new RegisterUserCommand("existing@b.com", "password123"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    }

    private static Mock<UserManager<IdentityUser<Guid>>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<IdentityUser<Guid>>>();
        return new Mock<UserManager<IdentityUser<Guid>>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }
}