using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Queries.GetAnticipationRequestById;

public sealed record GetAnticipationRequestByIdQuery(Guid Id) : IRequest<GetAnticipationRequestByIdResponse?>;
