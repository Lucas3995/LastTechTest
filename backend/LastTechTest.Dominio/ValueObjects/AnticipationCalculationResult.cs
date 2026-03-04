namespace LastTechTest.Dominio.ValueObjects;

public sealed record AnticipationCalculationResult(decimal GrossAmount, decimal FeesAmount, decimal NetAmount);