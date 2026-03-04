using FluentAssertions;

using LastTechTest.Aplicacao.Authentication.Queries.GetLoggedUser;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using Moq;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class GetLoggedUserQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly GetLoggedUserQueryHandler _sut;

    public GetLoggedUserQueryHandlerTests()
    {
        _sut = new GetLoggedUserQueryHandler(_currentUser.Object, _userRepo.Object);
    }

    [Fact]
    public async Task Handle_ValidUserId_ReturnsResponse()
    {
        var user = new User();
        user.SetEmail("u@t.com");
        var userId = user.Id;
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _userRepo.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.Handle(new GetLoggedUserQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be("u@t.com");
    }

    [Fact]
    public async Task Handle_NoUserInContext_Throws()
    {
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns((Guid?)null);

        var act = () => _sut.Handle(new GetLoggedUserQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*No logged user*");
    }

    [Fact]
    public async Task Handle_UserNotFound_Throws()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _userRepo.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var act = () => _sut.Handle(new GetLoggedUserQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*User not found*");
    }
}