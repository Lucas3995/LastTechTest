using LastTechTest.Dominio.Enums;

using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Commands.CancelAnticipationRequest;

public sealed record CancelAnticipationRequestCommand(Guid RequestId, string? Reason = null)
    : IRequest<CancelAnticipationRequestResponse>;

public sealed record CancelAnticipationRequestResponse(Guid Id, string Protocol, AnticipationRequestStatus Status, bool AlreadyCanceled = false);