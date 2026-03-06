using FluentAssertions;

using LastTechTest.Aplicacao.Anticipation.Queries.GetAnticipationRequestById;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using Moq;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class GetAnticipationRequestByIdQueryHandlerTests
{
    private readonly Mock<IAnticipationRequestRepository> _repository = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly GetAnticipationRequestByIdQueryHandler _sut;

    public GetAnticipationRequestByIdQueryHandlerTests()
    {
        _sut = new GetAnticipationRequestByIdQueryHandler(_repository.Object, _currentUser.Object);
    }

    /// <summary>CA4 – Admin: consulta por id deve retornar detalhes para qualquer solicitação.</summary>
    [Fact]
    public async Task Admin_WhenGettingById_Should_ReturnDetails_RegardlessOfCreator()
    {
        var adminUserId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var entity = AnticipationRequest.Create(creatorId, 100m, 100m, 2m, 98m);
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(adminUserId);
        _currentUser.Setup(x => x.GetRole()).Returns("Admin");
        _repository.Setup(x => x.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var query = new GetAnticipationRequestByIdQuery(entity.Id);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.CreatorId.Should().Be(creatorId);
        result.Status.Should().Be(nameof(Dominio.Enums.AnticipationRequestStatus.Pending));
        result.NetAmount.Should().Be(98m);
    }

    /// <summary>Creator + id próprio: deve retornar detalhes (CA2 happy path).</summary>
    [Fact]
    public async Task Creator_WhenGettingOwnRequest_Should_ReturnDetails()
    {
        var creatorId = Guid.NewGuid();
        var entity = AnticipationRequest.Create(creatorId, 100m, 100m, 2m, 98m);
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(creatorId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _repository.Setup(x => x.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var query = new GetAnticipationRequestByIdQuery(entity.Id);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.CreatorId.Should().Be(creatorId);
    }

    /// <summary>CA2 – Creator + id de outro: deve retornar erro de regra de negócio (400).</summary>
    [Fact]
    public async Task Creator_WhenGettingOtherCreatorRequest_Should_ThrowInvalidOperationException()
    {
        var creatorId = Guid.NewGuid();
        var otherCreatorId = Guid.NewGuid();
        var entity = AnticipationRequest.Create(otherCreatorId, 100m, 100m, 2m, 98m);
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(creatorId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _repository.Setup(x => x.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var query = new GetAnticipationRequestByIdQuery(entity.Id);
        var act = () => _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not allowed*");
    }

    /// <summary>Id inexistente: deve retornar null (404 na API).</summary>
    [Fact]
    public async Task WhenRequestNotFound_Should_ReturnNull()
    {
        var creatorId = Guid.NewGuid();
        var id = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(creatorId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((AnticipationRequest?)null);

        var query = new GetAnticipationRequestByIdQuery(id);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task WhenNotAuthenticated_Should_ThrowUnauthorizedAccessException()
    {
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns((Guid?)null);
        _repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((AnticipationRequest?)null);

        var query = new GetAnticipationRequestByIdQuery(Guid.NewGuid());
        var act = () => _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}