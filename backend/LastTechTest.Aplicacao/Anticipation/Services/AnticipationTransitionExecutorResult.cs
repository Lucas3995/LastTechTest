using LastTechTest.Dominio.Entities;

namespace LastTechTest.Aplicacao.Anticipation.Services;

/// <summary>RA-3: Result of executing a state transition. AlreadyCanceled is true only for Cancel when request was already canceled (idempotent).</summary>
public sealed record AnticipationTransitionExecutorResult(AnticipationRequest Entity, bool AlreadyCanceled);