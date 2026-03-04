using LastTechTest.Dominio.Enums;

namespace LastTechTest.Aplicacao.Anticipation.Commands.ConvertSimulationToRealRequest;

/// <summary>RA-4: Same contract as creation — Id, Protocol, NetAmount, Status of the created request.</summary>
public sealed record ConvertSimulationToRealRequestResponse(Guid Id, string Protocol, decimal NetAmount, AnticipationRequestStatus Status);
