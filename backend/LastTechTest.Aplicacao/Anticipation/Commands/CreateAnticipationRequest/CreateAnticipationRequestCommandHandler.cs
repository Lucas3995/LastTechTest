using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.ValueObjects;

using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Commands.CreateAnticipationRequest;

public sealed class CreateAnticipationRequestCommandHandler : IRequestHandler<CreateAnticipationRequestCommand, CreateAnticipationRequestResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAnticipationCalculationService _calculationService;
    private readonly IEligibilityService _eligibilityService;
    private readonly IAnticipationRequestRepository _repository;
    private readonly IReceivableRepository _receivableRepository;

    public CreateAnticipationRequestCommandHandler(
        ICurrentUserService currentUserService,
        IAnticipationCalculationService calculationService,
        IEligibilityService eligibilityService,
        IAnticipationRequestRepository repository,
        IReceivableRepository receivableRepository)
    {
        _currentUserService = currentUserService;
        _calculationService = calculationService;
        _eligibilityService = eligibilityService;
        _repository = repository;
        _receivableRepository = receivableRepository;
    }

    public async Task<CreateAnticipationRequestResponse> Handle(CreateAnticipationRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        var role = _currentUserService.GetRole();
        if (userId is null)
            throw new UnauthorizedAccessException("User not authenticated.");

        var creatorId = ResolveCreatorId(request.CreatorId, userId.Value, role);
        if (creatorId is null)
            throw new UnauthorizedAccessException("Creator is not allowed to act on behalf of another creator.");

        var hasPending = await _repository.HasPendingByCreatorAsync(creatorId.Value, cancellationToken);
        if (hasPending)
            throw new InvalidOperationException("Creator already has an open anticipation request.");

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
        var entity = AnticipationRequest.Create(creatorId.Value, request.RequestedAmount, result.GrossAmount, result.FeesAmount, result.NetAmount, requestedAt);
        await _repository.AddAsync(entity, cancellationToken);
        return new CreateAnticipationRequestResponse(entity.Id, entity.Protocol, entity.NetAmount, entity.Status);
    }

    private static Guid? ResolveCreatorId(Guid? requestCreatorId, Guid userId, string? role)
    {
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            return requestCreatorId ?? userId;
        if (string.Equals(role, "Creator", StringComparison.OrdinalIgnoreCase))
            return requestCreatorId.HasValue && requestCreatorId.Value != userId ? null : userId;
        return userId;
    }
}