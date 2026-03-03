using LastTechTest.Aplicacao.Common.Responses;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using MediatR;

namespace LastTechTest.Aplicacao.Authentication.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthTokensDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IUserPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthTokensDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var user = new User();
        user.SetEmail(request.Email);

        var hash = _passwordHasher.HashPassword(user, request.Password);
        user.SetPasswordHash(hash);

        await _userRepository.AddAsync(user, cancellationToken);

        var tokens = _tokenService.GenerateTokens(user);
        return new AuthTokensDto(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresAtUtc, tokens.RefreshTokenExpiresAtUtc);
    }
}

