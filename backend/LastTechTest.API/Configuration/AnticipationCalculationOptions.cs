namespace LastTechTest.API.Configuration;

public sealed class AnticipationCalculationOptions
{
    public const string SectionName = "Anticipation";

    public decimal FeeRate { get; set; } = 0.05m;
    public decimal DefaultCreatorLimit { get; set; } = 10_000m;
}
