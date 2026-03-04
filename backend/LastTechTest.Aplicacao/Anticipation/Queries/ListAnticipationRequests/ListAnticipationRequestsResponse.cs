namespace LastTechTest.Aplicacao.Anticipation.Queries.ListAnticipationRequests;

public sealed record ListAnticipationRequestsResponse(
    IReadOnlyList<AnticipationRequestListItem> Items,
    int TotalCount);