using MediatR;

namespace LastTechTest.Aplicacao.Anticipation.Commands.ConvertSimulationToRealRequest;

/// <summary>RA-4: Convert a cached simulation into a real anticipation request (identical values).</summary>
public sealed record ConvertSimulationToRealRequestCommand(string SimulationCode)
    : IRequest<ConvertSimulationToRealRequestResponse>;
