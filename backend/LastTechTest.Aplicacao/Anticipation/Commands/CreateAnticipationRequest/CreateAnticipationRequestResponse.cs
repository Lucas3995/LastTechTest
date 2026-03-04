using LastTechTest.Dominio.Enums;

namespace LastTechTest.Aplicacao.Anticipation.Commands.CreateAnticipationRequest;

public sealed record CreateAnticipationRequestResponse(Guid Id, string Protocol, decimal NetAmount, AnticipationRequestStatus Status);