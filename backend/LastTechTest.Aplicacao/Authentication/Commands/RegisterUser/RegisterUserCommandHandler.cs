using LastTechTest.Aplicacao.Common.Responses;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace LastTechTest.Aplicacao.Authentication.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthTokensDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    public RegisterUserCommandHandler(
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

    public async Task<AuthTokensDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedEmail = request.Email.Trim();

        var existingIdentityUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingIdentityUser is not null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var identityUser = new IdentityUser<Guid>
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true
        };

        var identityResult = await _userManager.CreateAsync(identityUser, request.Password);
        if (!identityResult.Succeeded)
        {
            throw new InvalidOperationException("Failed to create user: " +
                                                string.Join(", ", identityResult.Errors.Select(e => e.Description)));
        }

        var user = new User();
        user.SetEmail(normalizedEmail);
        var hash = _passwordHasher.HashPassword(user, request.Password);
        user.SetPasswordHash(hash);

        await _userRepository.AddAsync(user, cancellationToken);

        var tokens = _tokenService.GenerateTokens(user);

        var refreshTokenEntity = new UserToken();
        refreshTokenEntity.Initialize(user.Id, tokens.RefreshToken, TokenType.Refresh, tokens.RefreshTokenExpiresAtUtc);
        await _userTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        return new AuthTokensDto(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresAtUtc, tokens.RefreshTokenExpiresAtUtc);
    }
}