namespace LastTechTest.Aplicacao.Anticipation.Queries.ListAnticipationRequests;

public sealed record AnticipationRequestListItem(
    Guid Id,
    string Protocol,
    Guid CreatorId,
    string Status,
    decimal RequestedAmount,
    decimal NetAmount,
    DateTime RequestedAtUtc,
    DateTime CreatedAtUtc);
