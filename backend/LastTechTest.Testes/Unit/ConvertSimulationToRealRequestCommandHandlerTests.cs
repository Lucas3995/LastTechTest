using FluentAssertions;

using LastTechTest.Aplicacao.Anticipation.Commands.ConvertSimulationToRealRequest;
using LastTechTest.Aplicacao.Anticipation.Simulation;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.ValueObjects;

using Moq;

namespace LastTechTest.Testes.Unit;

/// <summary>RA-4 CA7–CA8: Convert simulation to real request — identical values, no open request, mark simulation as used.</summary>
[Trait("Category", "Unit")]
public class ConvertSimulationToRealRequestCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IAnticipationSimulationCache> _cache = new();
    private readonly Mock<IAnticipationRequestRepository> _repo = new();
    private readonly Mock<IAnticipationCalculationService> _calculation = new();
    private readonly Mock<IEligibilityService> _eligibility = new();
    private readonly Mock<IReceivableRepository> _receivableRepo = new();
    private readonly ConvertSimulationToRealRequestCommandHandler _sut;

    public ConvertSimulationToRealRequestCommandHandlerTests()
    {
        _sut = new ConvertSimulationToRealRequestCommandHandler(
            _currentUser.Object,
            _cache.Object,
            _repo.Object,
            _calculation.Object,
            _eligibility.Object,
            _receivableRepo.Object);
    }

    /// <summary>CA7: Valid code, cache returns simulation, no pending request — creates entity with identical values, calls AddAsync once, marks simulation as used.</summary>
    [Fact]
    public async Task Handle_ValidCode_NoPendingRequest_CreatesWithIdenticalValues_MarksSimulationUsed()
    {
        var userId = Guid.NewGuid();
        var creatorId = userId;
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        var requestedAt = DateTime.UtcNow.AddMinutes(-5);
        var simulationData = new SimulationData(creatorId, 1000m, 1000m, 50m, 950m, requestedAt);
        var realExpiry = DateTime.UtcNow.AddHours(1);
        _cache.Setup(x => x.GetAsync("VALID-CODE", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CachedSimulationEntry(simulationData, realExpiry, IsUsed: false));
        _repo.Setup(x => x.HasPendingByCreatorAsync(creatorId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _calculation.Setup(x => x.ValidateWithinCreatorLimit(creatorId, 1000m)).Returns((true, (string?)null));
        var receivables = new List<ReceivableInfo> { new(Guid.NewGuid(), 5000m, DateTime.UtcNow.AddDays(10), ReceivableStatus.Eligible) };
        _receivableRepo.Setup(x => x.GetEligibleByCreatorIdAsync(creatorId, It.IsAny<CancellationToken>())).ReturnsAsync(receivables);
        _eligibility.Setup(x => x.FilterEligible(receivables)).Returns(receivables);
        AnticipationRequest? captured = null;
        _repo.Setup(x => x.AddAsync(It.IsAny<AnticipationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AnticipationRequest, CancellationToken>((e, _) => captured = e)
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(new ConvertSimulationToRealRequestCommand("VALID-CODE"), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.NetAmount.Should().Be(950m);
        captured.Should().NotBeNull();
        captured!.CreatorId.Should().Be(creatorId);
        captured.RequestedAmount.Should().Be(1000m);
        captured.GrossAmount.Should().Be(1000m);
        captured.FeesAmount.Should().Be(50m);
        captured.NetAmount.Should().Be(950m);
        captured.RequestedAtUtc.Should().Be(requestedAt);
        _repo.Verify(x => x.AddAsync(It.IsAny<AnticipationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(x => x.MarkAsUsedAsync("VALID-CODE", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>CA8: Code not in cache (null) — does not call AddAsync, throws or returns error (e.g. NotFoundException / 404).</summary>
    [Fact]
    public async Task Handle_CodeNotFound_DoesNotCallAddAsync()
    {
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(Guid.NewGuid());
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _cache.Setup(x => x.GetAsync("MISSING", It.IsAny<CancellationToken>())).ReturnsAsync((CachedSimulationEntry?)null);

        var act = () => _sut.Handle(new ConvertSimulationToRealRequestCommand("MISSING"), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
        _repo.Verify(x => x.AddAsync(It.IsAny<AnticipationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _cache.Verify(x => x.MarkAsUsedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>CA8: Simulation already used (IsUsed true) — does not create, returns error (e.g. 422).</summary>
    [Fact]
    public async Task Handle_SimulationAlreadyUsed_DoesNotCallAddAsync()
    {
        var creatorId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(creatorId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        var data = new SimulationData(creatorId, 500m, 500m, 25m, 475m, DateTime.UtcNow);
        _cache.Setup(x => x.GetAsync("USED-CODE", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CachedSimulationEntry(data, DateTime.UtcNow.AddHours(1), IsUsed: true));

        var act = () => _sut.Handle(new ConvertSimulationToRealRequestCommand("USED-CODE"), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
        _repo.Verify(x => x.AddAsync(It.IsAny<AnticipationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>CA8: Creator has open request — does not create, returns error (e.g. already has open request).</summary>
    [Fact]
    public async Task Handle_CreatorHasPendingRequest_DoesNotCallAddAsync_ThrowsInvalidOperation()
    {
        var creatorId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(creatorId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        var data = new SimulationData(creatorId, 300m, 300m, 15m, 285m, DateTime.UtcNow);
        _cache.Setup(x => x.GetAsync("CODE", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CachedSimulationEntry(data, DateTime.UtcNow.AddHours(1), IsUsed: false));
        _repo.Setup(x => x.HasPendingByCreatorAsync(creatorId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.Handle(new ConvertSimulationToRealRequestCommand("CODE"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*open*");
        _repo.Verify(x => x.AddAsync(It.IsAny<AnticipationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _cache.Verify(x => x.MarkAsUsedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>CA8: Revalidation fails (e.g. limit changed) — does not create, returns validation error.</summary>
    [Fact]
    public async Task Handle_RevalidationFails_DoesNotCallAddAsync()
    {
        var creatorId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(creatorId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        var data = new SimulationData(creatorId, 100m, 100m, 5m, 95m, DateTime.UtcNow);
        _cache.Setup(x => x.GetAsync("CODE", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CachedSimulationEntry(data, DateTime.UtcNow.AddHours(1), IsUsed: false));
        _repo.Setup(x => x.HasPendingByCreatorAsync(creatorId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _calculation.Setup(x => x.ValidateWithinCreatorLimit(creatorId, 100m)).Returns((false, "Over limit"));

        var act = () => _sut.Handle(new ConvertSimulationToRealRequestCommand("CODE"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _repo.Verify(x => x.AddAsync(It.IsAny<AnticipationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>User not authenticated — throws UnauthorizedAccessException.</summary>
    [Fact]
    public async Task Handle_UserNotAuthenticated_ThrowsUnauthorized()
    {
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns((Guid?)null);

        var act = () => _sut.Handle(new ConvertSimulationToRealRequestCommand("CODE"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}

