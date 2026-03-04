using LastTechTest.Aplicacao.Anticipation.Simulation;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.ValueObjects;

using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Commands.SimulateAnticipationRequest;

/// <summary>RA-4: Simulates anticipation (no persistence). Uses same rules as CreateAnticipationRequest.</summary>
public sealed class SimulateAnticipationRequestCommandHandler : IRequestHandler<SimulateAnticipationRequestCommand, SimulateAnticipationRequestResponse>
{
    private const int RealTtlHours = 2;
    private const int ExposedValidityMinutesBeforeRealExpiry = 20;

    private readonly ICurrentUserService _currentUserService;
    private readonly IAnticipationCalculationService _calculationService;
    private readonly IEligibilityService _eligibilityService;
    private readonly IAnticipationSimulationCache _simulationCache;
    private readonly IReceivableRepository _receivableRepository;

    public SimulateAnticipationRequestCommandHandler(
        ICurrentUserService currentUserService,
        IAnticipationCalculationService calculationService,
        IEligibilityService eligibilityService,
        IAnticipationSimulationCache simulationCache,
        IReceivableRepository receivableRepository)
    {
        _currentUserService = currentUserService;
        _calculationService = calculationService;
        _eligibilityService = eligibilityService;
        _simulationCache = simulationCache;
        _receivableRepository = receivableRepository;
    }

    public async Task<SimulateAnticipationRequestResponse> Handle(SimulateAnticipationRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        var role = _currentUserService.GetRole();
        if (userId is null)
            throw new UnauthorizedAccessException("User not authenticated.");

        var creatorId = ResolveCreatorId(request.CreatorId, userId.Value, role);
        if (creatorId is null)
            throw new UnauthorizedAccessException("Creator is not allowed to act on behalf of another creator.");

        var (isValid, errorMessage) = _calculationService.ValidateWithinCreatorLimit(creatorId.Value, request.RequestedAmount);
        if (!isValid)
            throw new InvalidOperationException(errorMessage ?? "Validation failed.");

        var receivables = await _receivableRepository.GetEligibleByCreatorIdAsync(creatorId.Value, cancellationToken);
        var eligible = _eligibilityService.FilterEligible(receivables);
        if (eligible.Count == 0)
            throw new InvalidOperationException("No eligible receivables for this request.");

        var result = _calculationService.Calculate(request.RequestedAmount, eligible);
        if (result is null)
            throw new InvalidOperationException("Calculation failed: check amount and eligible receivables.");

        var requestedAt = request.RequestedAtUtc ?? DateTime.UtcNow;
        var data = new SimulationData(
            creatorId.Value,
            request.RequestedAmount,
            result.GrossAmount,
            result.FeesAmount,
            result.NetAmount,
            requestedAt);

        var ttl = TimeSpan.FromHours(RealTtlHours);
        var simulationCode = await _simulationCache.StoreAsync(creatorId.Value, data, ttl, cancellationToken);
        var realExpiresAt = DateTime.UtcNow.Add(ttl);
        var validUntilUtc = realExpiresAt.AddMinutes(-ExposedValidityMinutesBeforeRealExpiry);

        return new SimulateAnticipationRequestResponse(
            simulationCode,
            validUntilUtc,
            request.RequestedAmount,
            result.GrossAmount,
            result.FeesAmount,
            result.NetAmount);
    }

    private static Guid? ResolveCreatorId(Guid? requestCreatorId, Guid userId, string? role)
    {
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "Analista", StringComparison.OrdinalIgnoreCase))
            return requestCreatorId ?? userId;
        if (string.Equals(role, "Creator", StringComparison.OrdinalIgnoreCase))
            return requestCreatorId.HasValue && requestCreatorId.Value != userId ? null : userId;
        return userId;
    }
}
