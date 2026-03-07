using LastTechTest.Aplicacao.Common.Exceptions;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.Services;

namespace LastTechTest.Aplicacao.Anticipation.Services;

/// <summary>RA-3: Shared flow for approve/reject/cancel. Cancel when already canceled returns idempotent result; Approve/Reject same-state throw.</summary>
public sealed class AnticipationTransitionExecutor : IAnticipationTransitionExecutor
{
    private readonly IAnticipationRequestRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAnticipationAuditService _auditService;

    public AnticipationTransitionExecutor(
        IAnticipationRequestRepository repository,
        ICurrentUserService currentUserService,
        IAnticipationAuditService auditService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<AnticipationTransitionExecutorResult> ExecuteAsync(
        Guid requestId,
        string action,
        string? reasonOrObservation,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetCurrentUserId();
        var role = _currentUserService.GetRole();
        if (userId is null)
            throw new UnauthorizedAccessException("User not authenticated.");

        var entity = await _repository.GetByIdForUpdateAsync(requestId, cancellationToken);
        if (entity is null)
            throw new NotFoundException("Solicitação não encontrada.");

        if (AnticipationTransitionRules.IsSameStateTransition(entity.Status, action))
        {
            if (action == AnticipationTransitionAction.Cancel)
                return new AnticipationTransitionExecutorResult(entity, AlreadyCanceled: true);
            throw new InvalidOperationException(SameStateMessage(action));
        }

        var isOwner = entity.CreatorId == userId;
        ApplyTransition(entity, action, role, isOwner);
        await _auditService.RecordTransitionAsync(
            requestId,
            action,
            userId,
            DateTime.UtcNow,
            reasonOrObservation,
            cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new AnticipationTransitionExecutorResult(entity, AlreadyCanceled: false);
    }

    private static void ApplyTransition(AnticipationRequest entity, string action, string? role, bool isOwner)
    {
        switch (action)
        {
            case AnticipationTransitionAction.Approve:
                entity.Approve(role);
                break;
            case AnticipationTransitionAction.Reject:
                entity.Reject(role);
                break;
            case AnticipationTransitionAction.Cancel:
                entity.Cancel(role, isOwner);
                break;
        }
    }

    private static string SameStateMessage(string action)
    {
        return action switch
        {
            AnticipationTransitionAction.Approve => "A solicitação já foi aprovada anteriormente.",
            AnticipationTransitionAction.Reject => "A solicitação já foi recusada anteriormente.",
            _ => "A transição não é permitida para o estado atual."
        };
    }
}