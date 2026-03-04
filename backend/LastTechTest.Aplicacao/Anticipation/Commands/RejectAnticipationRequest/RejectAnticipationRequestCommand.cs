using LastTechTest.Dominio.Enums;

using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Commands.RejectAnticipationRequest;

public sealed record RejectAnticipationRequestCommand(Guid RequestId, string Reason)
    : IRequest<RejectAnticipationRequestResponse>;

public sealed record RejectAnticipationRequestResponse(Guid Id, string Protocol, AnticipationRequestStatus Status);
