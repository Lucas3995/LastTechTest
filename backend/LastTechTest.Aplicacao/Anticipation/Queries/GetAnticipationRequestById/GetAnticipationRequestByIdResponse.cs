namespace LastTechTest.Aplicacao.Anticipation.Queries.GetAnticipationRequestById;

public sealed record GetAnticipationRequestByIdResponse(
    Guid Id,
    string Protocol,
    Guid CreatorId,
    string Status,
    decimal RequestedAmount,
    decimal GrossAmount,
    decimal FeesAmount,
    decimal NetAmount,
    DateTime RequestedAtUtc,
    DateTime CreatedAtUtc);