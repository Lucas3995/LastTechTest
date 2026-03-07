using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

namespace LastTechTest.Infrastrutura;

public sealed class MfaService : IMfaService
{
    private readonly IKeyGenerator _keyGenerator;

    public MfaService(IKeyGenerator keyGenerator)
    {
        _keyGenerator = keyGenerator;
    }

    public Task<string> GenerateSecretAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        var secret = _keyGenerator.GenerateSecureToken(20);
        return Task.FromResult(secret);
    }

    public Task<bool> VerifyCodeAsync(User user, string code, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(code);

        // Stub simplificado para o primeiro ciclo.
        return Task.FromResult(true);
    }
}
