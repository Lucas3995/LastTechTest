using LastTechTest.Dominio.Entities;

namespace LastTechTest.Dominio.Interfaces;

public sealed record GeneratedTokens(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAtUtc, DateTime RefreshTokenExpiresAtUtc);

public interface ITokenService
{
    GeneratedTokens GenerateTokens(User user);

    Guid? GetUserIdFromExpiredAccessToken(string token);
}

