using FluentAssertions;

using LastTechTest.Aplicacao.Anticipation.Commands.CreateAnticipationRequest;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.ValueObjects;

using Moq;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class CreateAnticipationRequestCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IAnticipationCalculationService> _calculation = new();
    private readonly Mock<IEligibilityService> _eligibility = new();
    private readonly Mock<IAnticipationRequestRepository> _repo = new();
    private readonly Mock<IReceivableRepository> _receivableRepo = new();
    private readonly CreateAnticipationRequestCommandHandler _sut;

    public CreateAnticipationRequestCommandHandlerTests()
    {
        _sut = new CreateAnticipationRequestCommandHandler(
            _currentUser.Object,
            _calculation.Object,
            _eligibility.Object,
            _repo.Object,
            _receivableRepo.Object);
    }

    [Fact]
    public async Task Handle_Creator_ValidRequest_ReturnsResponse()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _repo.Setup(x => x.HasPendingByCreatorAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _calculation.Setup(x => x.ValidateWithinCreatorLimit(userId, 1000m)).Returns((true, (string?)null));
        var receivables = new List<ReceivableInfo> { new(Guid.NewGuid(), 5000m, DateTime.UtcNow.AddDays(10), ReceivableStatus.Eligible) };
        _receivableRepo.Setup(x => x.GetEligibleByCreatorIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(receivables);
        _eligibility.Setup(x => x.FilterEligible(receivables)).Returns(receivables);
        _calculation.Setup(x => x.Calculate(1000m, receivables)).Returns(new AnticipationCalculationResult(1000m, 50m, 950m));
        _repo.Setup(x => x.AddAsync(It.IsAny<AnticipationRequest>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.Handle(
            new CreateAnticipationRequestCommand(1000m, null, null),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.NetAmount.Should().Be(950m);
    }

    [Fact]
    public async Task Handle_UserNotAuthenticated_ThrowsUnauthorized()
    {
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns((Guid?)null);

        var act = () => _sut.Handle(new CreateAnticipationRequestCommand(1000m, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*not authenticated*");
    }

    [Fact]
    public async Task Handle_CreatorActsForAnother_ThrowsUnauthorized()
    {
        var userId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");

        var act = () => _sut.Handle(new CreateAnticipationRequestCommand(1000m, otherId, null), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*not allowed*");
    }

    [Fact]
    public async Task Handle_ValidationFails_ThrowsInvalidOperation()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _repo.Setup(x => x.HasPendingByCreatorAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _calculation.Setup(x => x.ValidateWithinCreatorLimit(userId, 50_000m)).Returns((false, "Over limit"));

        var act = () => _sut.Handle(new CreateAnticipationRequestCommand(50_000m, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenCreatorHasPendingRequest_ThrowsInvalidOperation()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _repo.Setup(x => x.HasPendingByCreatorAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.Handle(new CreateAnticipationRequestCommand(1000m, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*open anticipation request*");
    }

    [Fact]
    public async Task Handle_WhenCreatorHasNoPendingRequest_ContinuesToCreate()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _currentUser.Setup(x => x.GetRole()).Returns("Creator");
        _repo.Setup(x => x.HasPendingByCreatorAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _calculation.Setup(x => x.ValidateWithinCreatorLimit(userId, 101m)).Returns((true, (string?)null));
        var receivables = new List<ReceivableInfo> { new(Guid.NewGuid(), 500m, DateTime.UtcNow.AddDays(5), ReceivableStatus.Eligible) };
        _receivableRepo.Setup(x => x.GetEligibleByCreatorIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(receivables);
        _eligibility.Setup(x => x.FilterEligible(receivables)).Returns(receivables);
        _calculation.Setup(x => x.Calculate(101m, receivables)).Returns(new AnticipationCalculationResult(101m, 5.05m, 95.95m));
        _repo.Setup(x => x.AddAsync(It.IsAny<AnticipationRequest>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.Handle(new CreateAnticipationRequestCommand(101m, null, null), CancellationToken.None);

        result.Should().NotBeNull();
        result.NetAmount.Should().Be(95.95m);
    }
}