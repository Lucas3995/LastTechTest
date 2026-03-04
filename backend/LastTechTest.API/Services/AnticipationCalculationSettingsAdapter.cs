using LastTechTest.API.Configuration;
using LastTechTest.Dominio.Interfaces;

using Microsoft.Extensions.Options;

namespace LastTechTest.API.Services;

public sealed class AnticipationCalculationSettingsAdapter : IAnticipationCalculationSettings
{
    private readonly AnticipationCalculationOptions _options;

    public AnticipationCalculationSettingsAdapter(IOptions<AnticipationCalculationOptions> options)
    {
        _options = options.Value;
    }

    public decimal FeeRate => _options.FeeRate;
    public decimal DefaultCreatorLimit => _options.DefaultCreatorLimit;
}
