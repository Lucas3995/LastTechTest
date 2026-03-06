using FluentAssertions;

using LastTechTest.Aplicacao.Anticipation.Commands.SimulateAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Simulation;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.ValueObjects;

using Moq;

namespace LastTechTest.Testes.Unit;

/// <summary>RA-4 CA1–CA6: Simulation handler — same rules as RA-1, no persistence, cache store, validade exposta = real − 20 min.</summary>
[Trait("Category", "Unit")]
public class SimulateAnticipationRequestCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IAnticipationCalculationService> _calculation = new();
    private readonly Mock<IEligibilityService> _eligibility = new();
    private readonly Mock<IAnticipationSimulationCache> _cache = new();
    private readonly Mock<IReceivableRepository> _receivableRepo = new();
    private readonly SimulateAnticipationRequestCommandHandler _sut;

    public SimulateAnticipationRequestCommandHandlerTests()
    {
        _sut = new SimulateAnticipationRequestCommandHandler(
            _currentUser.Object,
            _calculation.Object,
            _eligibility.Object,
            _cache.Object,
            _receivableRepo.Object);
    }

    /// <summary>CA1: Creator, valid params — should return SimulationCode, ValidUntilUtc (real − 20 min), financial values; must NOT call repository AddAsync.</summary>
    [Fact]
    public async Task Handle_Creator_ValidRequest_ReturnsSimulationCodeAndExposedValidityAndFinancialValues_DoesNotCallRepository()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _calculation.Setup(x => x.ValidateWithinCreatorLimit(userId, 1000m)).Returns((true, (string?)null));
        var receivables = new List<ReceivableInfo> { new(Guid.NewGuid(), 5000m, DateTime.UtcNow.AddDays(10), ReceivableStatus.Eligible) };
        _receivableRepo.Setup(x => x.GetEligibleByCreatorIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(receivables);
        _eligibility.Setup(x => x.FilterEligible(receivables)).Returns(receivables);
        _calculation.Setup(x => x.Calculate(1000m, receivables)).Returns(new AnticipationCalculationResult(1000m, 50m, 950m));
        _cache.Setup(x => x.StoreAsync(userId, It.IsAny<SimulationData>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("SIM-CODE-1");

        var result = await _sut.Handle(
            new SimulateAnticipationRequestCommand(1000m, null, null),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.SimulationCode.Should().NotBeNullOrWhiteSpace();
        // CA6: validade exposta = real − 20 min (real TTL 2h)
        result.ValidUntilUtc.Should().BeCloseTo(DateTime.UtcNow.AddHours(2).AddMinutes(-20), TimeSpan.FromMinutes(2));
        result.RequestedAmount.Should().Be(1000m);
        result.GrossAmount.Should().Be(1000m);
        result.FeesAmount.Should().Be(50m);
        result.NetAmount.Should().Be(950m);
        _cache.Verify(x => x.StoreAsync(userId, It.IsAny<SimulationData>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>CA2: Violates business rules (e.g. limit) — validation fails, no cache store, no persistence.</summary>
    [Fact]
    public async Task Handle_ValidationFails_ThrowsInvalidOperation_DoesNotCallCacheStore()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _calculation.Setup(x => x.ValidateWithinCreatorLimit(userId, 50_000m)).Returns((false, "Over limit"));

        var act = () => _sut.Handle(new SimulateAnticipationRequestCommand(50_000m, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _cache.Verify(x => x.StoreAsync(It.IsAny<Guid>(), It.IsAny<SimulationData>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>CA3: Simulation must not call IAnticipationRequestRepository.AddAsync (no persistence).</summary>
    [Fact]
    public async Task Handle_ValidSimulation_DoesNotCallAnticipationRequestRepositoryAddAsync()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _calculation.Setup(x => x.ValidateWithinCreatorLimit(userId, 500m)).Returns((true, (string?)null));
        var receivables = new List<ReceivableInfo> { new(Guid.NewGuid(), 2000m, DateTime.UtcNow.AddDays(5), ReceivableStatus.Eligible) };
        _receivableRepo.Setup(x => x.GetEligibleByCreatorIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(receivables);
        _eligibility.Setup(x => x.FilterEligible(receivables)).Returns(receivables);
        _calculation.Setup(x => x.Calculate(500m, receivables)).Returns(new AnticipationCalculationResult(500m, 25m, 475m));
        _cache.Setup(x => x.StoreAsync(It.IsAny<Guid>(), It.IsAny<SimulationData>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).ReturnsAsync("code");

        await _sut.Handle(new SimulateAnticipationRequestCommand(500m, null, null), CancellationToken.None);

        // Handler does not receive IAnticipationRequestRepository — so no AddAsync possible. Test documents the requirement.
        _cache.Verify(x => x.StoreAsync(It.IsAny<Guid>(), It.IsAny<SimulationData>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>CA4: Analista/Admin can simulate on behalf of another creator (CreatorId in request).</summary>
    [Fact]
    public async Task Handle_Analista_WithCreatorId_ResolvesCreatorIdAndReturnsSimulation()
    {
        var analistaUserId = Guid.NewGuid();
        var targetCreatorId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(analistaUserId);
        _currentUser.Setup(x => x.GetRole()).Returns("Analista");
        _calculation.Setup(x => x.ValidateWithinCreatorLimit(targetCreatorId, 200m)).Returns((true, (string?)null));
        var receivables = new List<ReceivableInfo> { new(Guid.NewGuid(), 1000m, DateTime.UtcNow.AddDays(3), ReceivableStatus.Eligible) };
        _receivableRepo.Setup(x => x.GetEligibleByCreatorIdAsync(targetCreatorId, It.IsAny<CancellationToken>())).ReturnsAsync(receivables);
        _eligibility.Setup(x => x.FilterEligible(receivables)).Returns(receivables);
        _calculation.Setup(x => x.Calculate(200m, receivables)).Returns(new AnticipationCalculationResult(200m, 10m, 190m));
        _cache.Setup(x => x.StoreAsync(targetCreatorId, It.IsAny<SimulationData>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).ReturnsAsync("SIM-ANALISTA");

        var result = await _sut.Handle(new SimulateAnticipationRequestCommand(200m, targetCreatorId, null), CancellationToken.None);

        result.Should().NotBeNull();
        result.SimulationCode.Should().Be("SIM-ANALISTA");
        _cache.Verify(x => x.StoreAsync(targetCreatorId, It.IsAny<SimulationData>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>CA4: Admin with CreatorId stores simulation for that creator.</summary>
    [Fact]
    public async Task Handle_Admin_WithCreatorId_StoresForThatCreator()
    {
        var adminUserId = Guid.NewGuid();
        var targetCreatorId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(adminUserId);
        _currentUser.Setup(x => x.GetRole()).Returns("Admin");
        _calculation.Setup(x => x.ValidateWithinCreatorLimit(targetCreatorId, 300m)).Returns((true, (string?)null));
        var receivables = new List<ReceivableInfo> { new(Guid.NewGuid(), 1500m, DateTime.UtcNow.AddDays(7), ReceivableStatus.Eligible) };
        _receivableRepo.Setup(x => x.GetEligibleByCreatorIdAsync(targetCreatorId, It.IsAny<CancellationToken>())).ReturnsAsync(receivables);
        _eligibility.Setup(x => x.FilterEligible(receivables)).Returns(receivables);
        _calculation.Setup(x => x.Calculate(300m, receivables)).Returns(new AnticipationCalculationResult(300m, 15m, 285m));
        _cache.Setup(x => x.StoreAsync(targetCreatorId, It.IsAny<SimulationData>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).ReturnsAsync("SIM-ADMIN");

        var result = await _sut.Handle(new SimulateAnticipationRequestCommand(300m, targetCreatorId, null), CancellationToken.None);

        result.Should().NotBeNull();
        _cache.Verify(x => x.StoreAsync(targetCreatorId, It.IsAny<SimulationData>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>CA6: Two consecutive simulations for same creator — second replaces first (cache Store called twice with same creatorId).</summary>
    [Fact]
    public async Task Handle_TwoSimulationsSameCreator_SecondReplacesFirst_CacheStoreCalledTwiceForSameCreator()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _calculation.Setup(x => x.ValidateWithinCreatorLimit(userId, It.IsAny<decimal>())).Returns((true, (string?)null));
        var receivables = new List<ReceivableInfo> { new(Guid.NewGuid(), 5000m, DateTime.UtcNow.AddDays(10), ReceivableStatus.Eligible) };
        _receivableRepo.Setup(x => x.GetEligibleByCreatorIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(receivables);
        _eligibility.Setup(x => x.FilterEligible(receivables)).Returns(receivables);
        _calculation.Setup(x => x.Calculate(100m, receivables)).Returns(new AnticipationCalculationResult(100m, 5m, 95m));
        _calculation.Setup(x => x.Calculate(200m, receivables)).Returns(new AnticipationCalculationResult(200m, 10m, 190m));
        _cache.SetupSequence(x => x.StoreAsync(userId, It.IsAny<SimulationData>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("CODE-A")
            .ReturnsAsync("CODE-B");

        var first = await _sut.Handle(new SimulateAnticipationRequestCommand(100m, null, null), CancellationToken.None);
        var second = await _sut.Handle(new SimulateAnticipationRequestCommand(200m, null, null), CancellationToken.None);

        first.SimulationCode.Should().Be("CODE-A");
        second.SimulationCode.Should().Be("CODE-B");
        _cache.Verify(x => x.StoreAsync(userId, It.IsAny<SimulationData>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    /// <summary>User not authenticated — throws UnauthorizedAccessException.</summary>
    [Fact]
    public async Task Handle_UserNotAuthenticated_ThrowsUnauthorized()
    {
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns((Guid?)null);

        var act = () => _sut.Handle(new SimulateAnticipationRequestCommand(1000m, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*not authenticated*");
    }

    /// <summary>Creator acting for another (CreatorId different from self) — not allowed.</summary>
    [Fact]
    public async Task Handle_CreatorActsForAnother_ThrowsUnauthorized()
    {
        var userId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");

        var act = () => _sut.Handle(new SimulateAnticipationRequestCommand(1000m, otherId, null), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not allowed*");
    }

    /// <summary>Boundary: RequestedAmount 100 (minimum valid) — accepted when validation and calculation succeed.</summary>
    [Fact]
    public async Task Handle_RequestedAmount100_WhenValid_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _calculation.Setup(x => x.ValidateWithinCreatorLimit(userId, 100m)).Returns((true, (string?)null));
        var receivables = new List<ReceivableInfo> { new(Guid.NewGuid(), 500m, DateTime.UtcNow.AddDays(5), ReceivableStatus.Eligible) };
        _receivableRepo.Setup(x => x.GetEligibleByCreatorIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(receivables);
        _eligibility.Setup(x => x.FilterEligible(receivables)).Returns(receivables);
        _calculation.Setup(x => x.Calculate(100m, receivables)).Returns(new AnticipationCalculationResult(100m, 5m, 95m));
        _cache.Setup(x => x.StoreAsync(It.IsAny<Guid>(), It.IsAny<SimulationData>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).ReturnsAsync("MIN");

        var result = await _sut.Handle(new SimulateAnticipationRequestCommand(100m, null, null), CancellationToken.None);

        result.Should().NotBeNull();
        result.RequestedAmount.Should().Be(100m);
        result.NetAmount.Should().Be(95m);
    }

    /// <summary>No eligible receivables — throws InvalidOperationException.</summary>
    [Fact]
    public async Task Handle_NoEligibleReceivables_ThrowsInvalidOperation()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _calculation.Setup(x => x.ValidateWithinCreatorLimit(userId, 1000m)).Returns((true, (string?)null));
        var receivables = new List<ReceivableInfo>();
        _receivableRepo.Setup(x => x.GetEligibleByCreatorIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(receivables);
        _eligibility.Setup(x => x.FilterEligible(receivables)).Returns(receivables);

        var act = () => _sut.Handle(new SimulateAnticipationRequestCommand(1000m, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*eligible*");
    }
}