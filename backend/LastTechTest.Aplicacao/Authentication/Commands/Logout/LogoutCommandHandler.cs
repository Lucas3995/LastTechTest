using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IUserTokenRepository _userTokenRepository;

    public LogoutCommandHandler(IUserTokenRepository userTokenRepository)
    {
        _userTokenRepository = userTokenRepository;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _userTokenRepository.GetByValueAsync(request.RefreshToken, cancellationToken);
        if (token is null || token.Type != TokenType.Refresh)
        {
            return Unit.Value;
        }

        token.Revoke();
        await _userTokenRepository.UpdateAsync(token, cancellationToken);

        return Unit.Value;
    }
}