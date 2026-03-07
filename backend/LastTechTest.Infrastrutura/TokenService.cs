using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LastTechTest.Infrastrutura;

public sealed class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IKeyGenerator _keyGenerator;

    public TokenService(IConfiguration configuration, IKeyGenerator keyGenerator)
    {
        _configuration = configuration;
        _keyGenerator = keyGenerator;
    }

    public GeneratedTokens GenerateTokens(User user)
    {
        return GenerateTokens(user, null);
    }

    public GeneratedTokens GenerateTokens(User user, IEnumerable<string>? roles = null)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var secret = jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured.");
        var issuer = jwtSection["Issuer"] ?? "LastTechTest";
        var audience = jwtSection["Audience"] ?? "LastTechTest-Users";

        var accessMinutes = int.TryParse(jwtSection["AccessTokenMinutes"], out var a) ? a : 15;
        var refreshDays = int.TryParse(jwtSection["RefreshTokenDays"], out var r) ? r : 7;

        var accessExpires = DateTime.UtcNow.AddMinutes(accessMinutes);
        var refreshExpires = DateTime.UtcNow.AddDays(refreshDays);

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (roles is not null)
        {
            foreach (var role in roles.Where(r => !string.IsNullOrWhiteSpace(r)))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var tokenDescriptor = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: accessExpires,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        var refreshToken = _keyGenerator.GenerateSecureToken(64);

        return new GeneratedTokens(accessToken, refreshToken, accessExpires, refreshExpires);
    }

    public Guid? GetUserIdFromExpiredAccessToken(string token)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var secret = jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured.");
        var issuer = jwtSection["Issuer"] ?? "LastTechTest";
        var audience = jwtSection["Audience"] ?? "LastTechTest-Users";

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = false
        };

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(token, tokenValidationParameters, out _);
            var sub = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }
}
