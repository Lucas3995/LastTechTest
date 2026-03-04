using FluentAssertions;

using LastTechTest.Aplicacao.Authentication.Commands.ChangePassword;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using Microsoft.AspNetCore.Identity;
using Moq;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUserPasswordHasher> _passwordHasher = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<UserManager<IdentityUser<Guid>>> _userManager;
    private readonly ChangePasswordCommandHandler _sut;

    public ChangePasswordCommandHandlerTests()
    {
        _userManager = CreateUserManagerMock();

        _sut = new ChangePasswordCommandHandler(
            _userRepository.Object,
            _passwordHasher.Object,
            _currentUserService.Object,
            _userManager.Object);
    }

    [Fact]
    public async Task Handle_AuthenticatedUser_WithValidCurrentPassword_UpdatesPassword()
    {
        var userId = Guid.NewGuid();
        var user = new User();
        user.SetEmail("change@unit.test");
        user.SetPasswordHash("old-hash");

        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _userRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(x => x.HashPassword(user, "NewStrong1!")).Returns("new-hash");
        _userRepository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var identityUser = new IdentityUser<Guid> { Id = userId, Email = "change@unit.test" };
        _userManager.Setup(x => x.FindByEmailAsync("change@unit.test")).ReturnsAsync(identityUser);
        _userManager.Setup(x => x.ChangePasswordAsync(identityUser, "Current123!", "NewStrong1!"))
            .ReturnsAsync(IdentityResult.Success);

        var command = new ChangePasswordCommand("Current123!", "NewStrong1!");

        await _sut.Handle(command, CancellationToken.None);
        user.PasswordHash.Should().Be("new-hash");
        _userRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ThrowsUnauthorized()
    {
        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns((Guid?)null);

        var command = new ChangePasswordCommand("Current123!", "NewStrong1!");

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not authenticated*");
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordIsInvalid_ThrowsInvalidOperation()
    {
        var userId = Guid.NewGuid();
        var user = new User();
        user.SetEmail("change@unit.test");
        user.SetPasswordHash("old-hash");

        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _userRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var identityUser = new IdentityUser<Guid> { Id = userId, Email = "change@unit.test" };
        _userManager.Setup(x => x.FindByEmailAsync("change@unit.test")).ReturnsAsync(identityUser);
        _userManager.Setup(x => x.ChangePasswordAsync(identityUser, "Wrong123!", "NewStrong1!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordMismatch",
                Description = "Current password is invalid."
            }));

        var command = new ChangePasswordCommand("Wrong123!", "NewStrong1!");

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Current password is invalid*");
    }

    [Fact]
    public async Task Handle_WhenIdentityUserNotFound_ThrowsInvalidOperation()
    {
        var userId = Guid.NewGuid();
        var user = new User();
        user.SetEmail("missing@unit.test");
        user.SetPasswordHash("old-hash");

        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _userRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userManager.Setup(x => x.FindByEmailAsync("missing@unit.test")).ReturnsAsync((IdentityUser<Guid>?)null);

        var command = new ChangePasswordCommand("Current123!", "NewStrong1!");

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*User identity not found*");
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

