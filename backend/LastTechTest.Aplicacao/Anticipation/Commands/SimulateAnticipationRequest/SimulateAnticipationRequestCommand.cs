using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Commands.SimulateAnticipationRequest;

/// <summary>RA-4: Simulate anticipation without persisting. CreatorId optional for Admin/Analista (on behalf of).</summary>
public sealed record SimulateAnticipationRequestCommand(
    decimal RequestedAmount,
    Guid? CreatorId = null,
    DateTime? RequestedAtUtc = null) : IRequest<SimulateAnticipationRequestResponse>;

