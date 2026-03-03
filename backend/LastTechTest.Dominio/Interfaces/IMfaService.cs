using LastTechTest.Dominio.Entities;

namespace LastTechTest.Dominio.Interfaces;

public interface IMfaService
{
    Task<string> GenerateSecretAsync(User user, CancellationToken cancellationToken = default);

    Task<bool> VerifyCodeAsync(User user, string code, CancellationToken cancellationToken = default);
}