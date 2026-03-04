using FluentAssertions;

using LastTechTest.Aplicacao.Anticipation.Queries.ListAnticipationRequests;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Enums;
using LastTechTest.Dominio.Interfaces;

using Moq;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class ListAnticipationRequestsQueryHandlerTests
{
    private readonly Mock<IAnticipationRequestRepository> _repository = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly ListAnticipationRequestsQueryHandler _sut;

    public ListAnticipationRequestsQueryHandlerTests()
    {
        _sut = new ListAnticipationRequestsQueryHandler(_repository.Object, _currentUser.Object);
    }

    /// <summary>CA1 – Creator: listagem deve ignorar filtro CreatorId do request e usar creator_id do token.</summary>
    [Fact]
    public async Task Creator_WhenListing_Should_IgnoreRequestCreatorFilter_AndReturnOnlyOwn()
    {
        var creatorId = Guid.NewGuid();
        var otherCreatorId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(creatorId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");

        var entity = AnticipationRequest.Create(creatorId, 100m, 100m, 2m, 98m);
        _repository
            .Setup(x => x.ListAsync(creatorId, null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AnticipationRequest> { entity }, 1));

        var query = new ListAnticipationRequestsQuery(CreatorId: otherCreatorId, Status: null, FromUtc: null, ToUtc: null, Page: 1, PageSize: 20);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].CreatorId.Should().Be(creatorId);
        _repository.Verify(
            x => x.ListAsync(creatorId, null, null, null, 1, 20, It.IsAny<CancellationToken>()),
            Times.Once,
            "Creator must force creator_id from token, ignoring request filter");
    }

    /// <summary>CA3 – Admin: listagem deve aceitar filtros (creator, status, período).</summary>
    [Fact]
    public async Task Admin_WhenListing_Should_ApplyFilters()
    {
        var adminUserId = Guid.NewGuid();
        var filterCreatorId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(adminUserId);
        _currentUser.Setup(x => x.GetRole()).Returns("Admin");

        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;
        var entity = AnticipationRequest.Create(filterCreatorId, 200m, 200m, 4m, 196m);
        _repository
            .Setup(x => x.ListAsync(filterCreatorId, AnticipationRequestStatus.Created, from, to, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AnticipationRequest> { entity }, 1));

        var query = new ListAnticipationRequestsQuery(
            CreatorId: filterCreatorId,
            Status: (int)AnticipationRequestStatus.Created,
            FromUtc: from,
            ToUtc: to,
            Page: 1,
            PageSize: 20);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].CreatorId.Should().Be(filterCreatorId);
        result.Items[0].Status.Should().Be(nameof(AnticipationRequestStatus.Created));
        _repository.Verify(
            x => x.ListAsync(filterCreatorId, AnticipationRequestStatus.Created, from, to, 1, 20, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>CA5 – Paginação: handler deve repassar page e pageSize e retornar TotalCount e Items.</summary>
    [Fact]
    public async Task Pagination_Should_PassPageAndPageSize_AndReturnTotalCountAndItems()
    {
        var creatorId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(creatorId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");

        var items = new List<AnticipationRequest>
        {
            AnticipationRequest.Create(creatorId, 100m, 100m, 2m, 98m),
            AnticipationRequest.Create(creatorId, 200m, 200m, 4m, 196m),
        };
        _repository
            .Setup(x => x.ListAsync(creatorId, null, null, null, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 10));

        var query = new ListAnticipationRequestsQuery(null, null, null, null, Page: 2, PageSize: 5);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(10);
        result.Items.Should().HaveCount(2);
        _repository.Verify(
            x => x.ListAsync(creatorId, null, null, null, 2, 5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WhenNotAuthenticated_Should_ThrowUnauthorizedAccessException()
    {
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns((Guid?)null);

        var query = new ListAnticipationRequestsQuery(null, null, null, null, 1, 20);
        var act = () => _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    /// <summary>Listagem sem itens deve retornar lista vazia e TotalCount 0.</summary>
    [Fact]
    public async Task WhenNoRequests_Should_ReturnEmptyListAndZeroTotal()
    {
        var creatorId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(creatorId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");

        _repository
            .Setup(x => x.ListAsync(creatorId, null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AnticipationRequest>(), 0));

        var query = new ListAnticipationRequestsQuery(null, null, null, null, 1, 20);
        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}