using LastTechTest.Dominio.Authorization;
using LastTechTest.Dominio.Enums;
using Action = LastTechTest.Dominio.AnticipationTransitionAction;

namespace LastTechTest.Dominio.Services;

/// <summary>
/// RA-3: Centralized transition rules for anticipation requests.
/// "Analysis pending" = Created or Pending (ANALISE_PENDENTE in demand).
/// </summary>
public static class AnticipationTransitionRules
{
    private static bool IsAnalysisPending(AnticipationRequestStatus status)
    {
        return status is AnticipationRequestStatus.Created or AnticipationRequestStatus.Pending;
    }

    /// <summary>RC-1: Public predicate for "em análise" (Created or Pending). Use in repository for HasPendingByCreatorAsync.</summary>
    public static bool IsAnalysisPendingStatus(AnticipationRequestStatus status) => IsAnalysisPending(status);

    /// <summary>Can the user approve from the current state? (Analista or Admin only; only from analysis pending.)</summary>
    public static bool CanApprove(AnticipationRequestStatus currentState, string? role)
    {
        if (!IsAnalysisPending(currentState)) return false;
        return string.Equals(role, KnownRoles.Analista, StringComparison.OrdinalIgnoreCase)
               || string.Equals(role, KnownRoles.Admin, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Can the user reject from the current state? (Analista or Admin only; only from analysis pending.)</summary>
    public static bool CanReject(AnticipationRequestStatus currentState, string? role)
    {
        if (!IsAnalysisPending(currentState)) return false;
        return string.Equals(role, KnownRoles.Analista, StringComparison.OrdinalIgnoreCase)
               || string.Equals(role, KnownRoles.Admin, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Can the user cancel? Creator only for own request when analysis pending; Admin can cancel from analysis pending.</summary>
    public static bool CanCancel(AnticipationRequestStatus currentState, string? role, bool isOwner)
    {
        if (!IsAnalysisPending(currentState)) return false;
        if (string.Equals(role, KnownRoles.Admin, StringComparison.OrdinalIgnoreCase)) return true;
        if (string.Equals(role, KnownRoles.Creator, StringComparison.OrdinalIgnoreCase) && isOwner) return true;
        return false;
    }

    /// <summary>Is the transition to the same state (idempotent no-op)? e.g. cancel when already CanceledByCreator.</summary>
    public static bool IsSameStateTransition(AnticipationRequestStatus currentState, string action)
    {
        return action switch
        {
            Action.Approve => currentState == AnticipationRequestStatus.Approved,
            Action.Reject => currentState == AnticipationRequestStatus.Rejected,
            Action.Cancel => currentState == AnticipationRequestStatus.CanceledByCreator,
            _ => false
        };
    }

    /// <summary>Returns the next status if the transition is allowed; null otherwise.</summary>
    public static AnticipationRequestStatus? GetNextState(
        AnticipationRequestStatus currentState,
        string action,
        string? role,
        bool isOwner)
    {
        return action switch
        {
            Action.Approve => CanApprove(currentState, role) ? AnticipationRequestStatus.Approved : null,
            Action.Reject => CanReject(currentState, role) ? AnticipationRequestStatus.Rejected : null,
            Action.Cancel => CanCancel(currentState, role, isOwner) ? AnticipationRequestStatus.CanceledByCreator : null,
            _ => null
        };
    }
}
