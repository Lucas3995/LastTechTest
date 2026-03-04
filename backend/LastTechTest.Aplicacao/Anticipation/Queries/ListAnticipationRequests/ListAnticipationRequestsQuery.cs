using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Queries.ListAnticipationRequests;

public sealed record ListAnticipationRequestsQuery(
    Guid? CreatorId,
    int? Status,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Page = 1,
    int PageSize = 20) : IRequest<ListAnticipationRequestsResponse>;
