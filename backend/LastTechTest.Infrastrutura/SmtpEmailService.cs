using LastTechTest.Dominio.Interfaces;

using Microsoft.Extensions.Configuration;

namespace LastTechTest.Infrastrutura;

public sealed class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        // Implementação stub; pode ser ligada a SMTP real em ciclos futuros.
        _ = to;
        _ = subject;
        _ = body;
        _ = cancellationToken;
        return Task.CompletedTask;
    }
}
