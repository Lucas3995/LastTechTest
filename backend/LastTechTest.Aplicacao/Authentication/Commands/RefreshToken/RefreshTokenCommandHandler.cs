using LastTechTest.Aplicacao.Common.Responses;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthTokensDto>
{
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IUserTokenRepository userTokenRepository,
        IUserRepository userRepository,
        ITokenService tokenService)
    {
        _userTokenRepository = userTokenRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthTokensDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _userTokenRepository.GetByValueAsync(request.RefreshToken, cancellationToken);
        if (existingToken is null || existingToken.Type != TokenType.Refresh || !existingToken.IsActiveAt(DateTime.UtcNow))
        {
            throw new InvalidOperationException("Invalid refresh token.");
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("Invalid refresh token.");
        }

        existingToken.Revoke();
        await _userTokenRepository.UpdateAsync(existingToken, cancellationToken);

        var tokens = _tokenService.GenerateTokens(user);

        var refreshTokenEntity = new UserToken();
        refreshTokenEntity.Initialize(user.Id, tokens.RefreshToken, TokenType.Refresh, tokens.RefreshTokenExpiresAtUtc);
        await _userTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        return new AuthTokensDto(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresAtUtc, tokens.RefreshTokenExpiresAtUtc);
    }
}

