using LastTechTest.Aplicacao.Common.Responses;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthTokensDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUserTokenRepository _userTokenRepository;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUserPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUserTokenRepository userTokenRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _userTokenRepository = userTokenRepository;
    }

    public async Task<AuthTokensDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        var passwordValid = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (!passwordValid)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        var tokens = _tokenService.GenerateTokens(user);

        var refreshTokenEntity = new UserToken();
        refreshTokenEntity.Initialize(user.Id, tokens.RefreshToken, TokenType.Refresh, tokens.RefreshTokenExpiresAtUtc);
        await _userTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        user.MarkLoggedIn(DateTime.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new AuthTokensDto(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresAtUtc, tokens.RefreshTokenExpiresAtUtc);
    }
}