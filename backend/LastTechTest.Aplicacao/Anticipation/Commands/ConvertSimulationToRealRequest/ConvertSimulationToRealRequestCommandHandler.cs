using LastTechTest.Aplicacao.Anticipation.Simulation;
using LastTechTest.Aplicacao.Common.Exceptions;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.ValueObjects;

using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Commands.ConvertSimulationToRealRequest;

/// <summary>RA-4: Converts cached simulation to real request (identical values). Revalidates rules; no open request.</summary>
public sealed class ConvertSimulationToRealRequestCommandHandler : IRequestHandler<ConvertSimulationToRealRequestCommand, ConvertSimulationToRealRequestResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAnticipationSimulationCache _simulationCache;
    private readonly IAnticipationRequestRepository _repository;
    private readonly IAnticipationCalculationService _calculationService;
    private readonly IEligibilityService _eligibilityService;
    private readonly IReceivableRepository _receivableRepository;

    public ConvertSimulationToRealRequestCommandHandler(
        ICurrentUserService currentUserService,
        IAnticipationSimulationCache simulationCache,
        IAnticipationRequestRepository repository,
        IAnticipationCalculationService calculationService,
        IEligibilityService eligibilityService,
        IReceivableRepository receivableRepository)
    {
        _currentUserService = currentUserService;
        _simulationCache = simulationCache;
        _repository = repository;
        _calculationService = calculationService;
        _eligibilityService = eligibilityService;
        _receivableRepository = receivableRepository;
    }

    public async Task<ConvertSimulationToRealRequestResponse> Handle(ConvertSimulationToRealRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        var role = _currentUserService.GetRole();
        if (userId is null)
            throw new UnauthorizedAccessException("User not authenticated.");

        var entry = await _simulationCache.GetAsync(request.SimulationCode, cancellationToken);
        if (entry is null)
            throw new NotFoundException("Simulation not found or expired.");
        if (entry.IsUsed)
            throw new InvalidOperationException("Simulation already used.");

        var creatorId = entry.Data.CreatorId;
        if (!CanConvert(creatorId, userId.Value, role))
            throw new UnauthorizedAccessException("Only the creator who simulated or Admin can convert this simulation.");

        var hasPending = await _repository.HasPendingByCreatorAsync(creatorId, cancellationToken);
        if (hasPending)
            throw new InvalidOperationException("Creator already has an open anticipation request.");

        var (isValid, errorMessage) = _calculationService.ValidateWithinCreatorLimit(creatorId, entry.Data.RequestedAmount);
        if (!isValid)
            throw new InvalidOperationException(errorMessage ?? "Validation failed.");

        var receivables = await _receivableRepository.GetEligibleByCreatorIdAsync(creatorId, cancellationToken);
        var eligible = _eligibilityService.FilterEligible(receivables);
        if (eligible.Count == 0)
            throw new InvalidOperationException("No eligible receivables for this request.");

        var entity = AnticipationRequest.Create(
            creatorId,
            entry.Data.RequestedAmount,
            entry.Data.GrossAmount,
            entry.Data.FeesAmount,
            entry.Data.NetAmount,
            entry.Data.RequestedAtUtc);

        await _repository.AddAsync(entity, cancellationToken);
        await _simulationCache.MarkAsUsedAsync(request.SimulationCode, cancellationToken);

        return new ConvertSimulationToRealRequestResponse(entity.Id, entity.Protocol, entity.NetAmount, entity.Status);
    }

    private static bool CanConvert(Guid simulationCreatorId, Guid currentUserId, string? role)
    {
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(role, "Creator", StringComparison.OrdinalIgnoreCase))
            return simulationCreatorId == currentUserId;
        return false;
    }
}