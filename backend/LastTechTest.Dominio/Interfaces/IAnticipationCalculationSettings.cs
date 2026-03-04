namespace LastTechTest.Dominio.Interfaces;

/// <summary>Configuration for anticipation calculation (fee rate and creator limit). Implemented at the edge (API/Infrastructure) via Options or IConfiguration; domain does not depend on Microsoft.Extensions.Options.</summary>
public interface IAnticipationCalculationSettings
{
    decimal FeeRate { get; }
    decimal DefaultCreatorLimit { get; }
}