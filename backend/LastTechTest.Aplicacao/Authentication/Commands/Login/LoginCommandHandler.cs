using LastTechTest.Aplicacao.Common.Responses;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace LastTechTest.Aplicacao.Authentication.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthTokensDto>
{
    /// <summary>Default domain when user logs in with a login name only (e.g. usu_acesso_total). Must match IdentitySeeder admin email.</summary>
    private const string DefaultLoginDomain = "example.com";

    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUserPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUserTokenRepository userTokenRepository,
        UserManager<IdentityUser<Guid>> userManager)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _userTokenRepository = userTokenRepository;
        _userManager = userManager;
    }

    public async Task<AuthTokensDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginInput = request.Email.Trim();
        var lookupEmail = loginInput.Contains('@', StringComparison.Ordinal)
            ? loginInput
            : loginInput + "@" + DefaultLoginDomain;

        var user = await _userRepository.GetByEmailAsync(lookupEmail, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        var passwordValid = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (!passwordValid)
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        IEnumerable<string>? roles = null;

        var identityUser = await _userManager.FindByEmailAsync(user.Email);
        if (identityUser is not null)
        {
            roles = await _userManager.GetRolesAsync(identityUser);
        }

        var tokens = _tokenService.GenerateTokens(user, roles);

        var refreshTokenEntity = new UserToken();
        refreshTokenEntity.Initialize(user.Id, tokens.RefreshToken, TokenType.Refresh, tokens.RefreshTokenExpiresAtUtc);
        await _userTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        user.MarkLoggedIn(DateTime.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new AuthTokensDto(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresAtUtc, tokens.RefreshTokenExpiresAtUtc);
    }
}