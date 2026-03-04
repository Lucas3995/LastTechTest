namespace LastTechTest.Aplicacao.Anticipation.Commands.SimulateAnticipationRequest;

/// <summary>RA-4: Simulation result with code, exposed validity (real expiry − 20 min), and financial values.</summary>
public sealed record SimulateAnticipationRequestResponse(
    string SimulationCode,
    DateTime ValidUntilUtc,
    decimal RequestedAmount,
    decimal GrossAmount,
    decimal FeesAmount,
    decimal NetAmount);
