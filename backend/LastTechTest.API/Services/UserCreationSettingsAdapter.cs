using LastTechTest.API.Configuration;
using LastTechTest.Aplicacao.Common.Interfaces;

using Microsoft.Extensions.Options;

namespace LastTechTest.API.Services;

public sealed class UserCreationSettingsAdapter : IUserCreationSettings
{
    private readonly UserCreationOptions _options;

    public UserCreationSettingsAdapter(IOptions<UserCreationOptions> options)
    {
        _options = options.Value;
    }

    public string DefaultPassword => _options.DefaultPassword;
}
