namespace LastTechTest.API.Dtos;

/// <summary>Contrato de criação de solicitação de antecipação. Apenas 3 campos (padrão .NET PascalCase): RequestedAmount, CreatorId (opcional), RequestedAtUtc (opcional).</summary>
public sealed record CreateAnticipationRequestDto(decimal RequestedAmount, Guid? CreatorId, DateTime? RequestedAtUtc);

/// <summary>RA-3: body para POST approve. Observation opcional.</summary>
public sealed record ApproveAnticipationRequestDto(string? Observation);

/// <summary>RA-3: body para POST reject. Reason é obrigatório (validado no endpoint; ausência devolve 400 com "Reason is required.").</summary>
public sealed record RejectAnticipationRequestDto(string? Reason);

/// <summary>RA-3: body para POST cancel. Reason opcional.</summary>
public sealed record CancelAnticipationRequestDto(string? Reason);

/// <summary>RA-4: body para POST simulations. Same shape as create (RequestedAmount, CreatorId optional, RequestedAtUtc optional).</summary>
public sealed record SimulateAnticipationRequestDto(decimal RequestedAmount, Guid? CreatorId, DateTime? RequestedAtUtc);