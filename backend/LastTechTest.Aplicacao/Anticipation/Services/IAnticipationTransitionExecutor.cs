namespace LastTechTest.Aplicacao.Anticipation.Services;

/// <summary>RA-3: Executes a single anticipation request state transition (approve, reject, cancel). Centralizes load, validate, apply, audit, save.</summary>
public interface IAnticipationTransitionExecutor
{
    /// <summary>Executes the transition. For Cancel when already canceled, returns with AlreadyCanceled true; for Approve/Reject in same state, throws InvalidOperationException.</summary>
    Task<AnticipationTransitionExecutorResult> ExecuteAsync(
        Guid requestId,
        string action,
        string? reasonOrObservation,
        CancellationToken cancellationToken = default);
}