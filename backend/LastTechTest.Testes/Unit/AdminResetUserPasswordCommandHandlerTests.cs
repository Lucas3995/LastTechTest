using FluentAssertions;

using LastTechTest.Aplicacao.Authentication.Commands.AdminResetUserPassword;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using Microsoft.AspNetCore.Identity;

using Moq;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class AdminResetUserPasswordCommandHandlerTests
{
    private const string DefaultPassword = "Trocar@123";

    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUserPasswordHasher> _passwordHasher = new();
    private readonly Mock<UserManager<IdentityUser<Guid>>> _userManager;
    private readonly AdminResetUserPasswordCommandHandler _sut;

    public AdminResetUserPasswordCommandHandlerTests()
    {
        _userManager = CreateUserManagerMock();
        _sut = new AdminResetUserPasswordCommandHandler(
            _userRepository.Object,
            _passwordHasher.Object,
            _userManager.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFoundById_ThrowsInvalidOperation()
    {
        var id = Guid.NewGuid();
        _userRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var command = new AdminResetUserPasswordCommand(null, id);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*User not found*");
    }

    [Fact]
    public async Task Handle_WhenUserNotFoundByEmail_ThrowsInvalidOperation()
    {
        _userRepository.Setup(x => x.GetByEmailAsync("missing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new AdminResetUserPasswordCommand("missing@example.com", null);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*User not found*");
    }

    [Fact]
    public async Task Handle_WhenIdentityUserNotFound_ThrowsInvalidOperation()
    {
        var user = new User();
        user.SetEmail("no-identity@example.com");
        user.SetPasswordHash("old-hash");
        _userRepository.Setup(x => x.GetByEmailAsync("no-identity@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userManager.Setup(x => x.FindByEmailAsync("no-identity@example.com"))
            .ReturnsAsync((IdentityUser<Guid>?)null);

        var command = new AdminResetUserPasswordCommand("no-identity@example.com", null);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*User identity not found*");
    }

    [Fact]
    public async Task Handle_WhenIdentifiedById_ResetsPassword_InIdentityAndDomain()
    {
        var userId = Guid.NewGuid();
        var user = new User();
        user.SetEmail("reset-by-id@example.com");
        user.SetPasswordHash("old-hash");

        _userRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var identityUser = new IdentityUser<Guid> { Id = Guid.NewGuid(), Email = "reset-by-id@example.com" };
        _userManager.Setup(x => x.FindByEmailAsync("reset-by-id@example.com")).ReturnsAsync(identityUser);
        _userManager.Setup(x => x.RemovePasswordAsync(identityUser)).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(x => x.AddPasswordAsync(identityUser, DefaultPassword)).ReturnsAsync(IdentityResult.Success);

        _passwordHasher.Setup(x => x.HashPassword(user, DefaultPassword)).Returns("new-hash");

        var command = new AdminResetUserPasswordCommand(null, userId);

        await _sut.Handle(command, CancellationToken.None);

        user.PasswordHash.Should().Be("new-hash");
        _userManager.Verify(x => x.RemovePasswordAsync(identityUser), Times.Once);
        _userManager.Verify(x => x.AddPasswordAsync(identityUser, DefaultPassword), Times.Once);
        _userRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenIdentifiedByEmail_ResetsPassword_InIdentityAndDomain()
    {
        var user = new User();
        user.SetEmail("reset-by-email@example.com");
        user.SetPasswordHash("old-hash");

        _userRepository.Setup(x => x.GetByEmailAsync("reset-by-email@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var identityUser = new IdentityUser<Guid> { Id = Guid.NewGuid(), Email = "reset-by-email@example.com" };
        _userManager.Setup(x => x.FindByEmailAsync("reset-by-email@example.com")).ReturnsAsync(identityUser);
        _userManager.Setup(x => x.RemovePasswordAsync(identityUser)).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(x => x.AddPasswordAsync(identityUser, DefaultPassword)).ReturnsAsync(IdentityResult.Success);

        _passwordHasher.Setup(x => x.HashPassword(user, DefaultPassword)).Returns("new-hash");

        var command = new AdminResetUserPasswordCommand("reset-by-email@example.com", null);

        await _sut.Handle(command, CancellationToken.None);

        user.PasswordHash.Should().Be("new-hash");
        _userManager.Verify(x => x.RemovePasswordAsync(identityUser), Times.Once);
        _userManager.Verify(x => x.AddPasswordAsync(identityUser, DefaultPassword), Times.Once);
        _userRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
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