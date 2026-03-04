using LastTechTest.Dominio.Enums;

namespace LastTechTest.Dominio.ValueObjects;

/// <summary>Parameter object for list anticipation requests (RA-2). Single source for filter and pagination.</summary>
public sealed record ListAnticipationRequestsFilter(
    Guid? CreatorId,
    AnticipationRequestStatus? Status,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Page = 1,
    int PageSize = 20);