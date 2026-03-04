using LastTechTest.Dominio.Enums;

using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Commands.ApproveAnticipationRequest;

public sealed record ApproveAnticipationRequestCommand(Guid RequestId, string? Observation = null)
    : IRequest<ApproveAnticipationRequestResponse>;

public sealed record ApproveAnticipationRequestResponse(Guid Id, string Protocol, AnticipationRequestStatus Status);