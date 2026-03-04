using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Commands.CreateAnticipationRequest;

public sealed record CreateAnticipationRequestCommand(
    decimal RequestedAmount,
    Guid? CreatorId = null,
    DateTime? RequestedAtUtc = null) : IRequest<CreateAnticipationRequestResponse>;
