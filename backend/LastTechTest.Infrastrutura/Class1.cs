using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
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
            var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }
}

public interface IKeyGenerator
{
    string GenerateSecureToken(int length);
}

public sealed class KeyGenerator : IKeyGenerator
{
    public string GenerateSecureToken(int length)
    {
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        return WebEncoders.Base64UrlEncode(bytes);
    }
}

public sealed class PasswordHasher : IUserPasswordHasher
{
    public string HashPassword(User user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = Guid.NewGuid().ToByteArray();
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        var result = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, result, salt.Length, hash.Length);

        return Convert.ToBase64String(result);
    }

    public bool VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(hashedPassword);
        ArgumentException.ThrowIfNullOrEmpty(providedPassword);

        var decoded = Convert.FromBase64String(hashedPassword);
        var salt = new byte[16];
        var storedHash = new byte[decoded.Length - salt.Length];

        Buffer.BlockCopy(decoded, 0, salt, 0, salt.Length);
        Buffer.BlockCopy(decoded, salt.Length, storedHash, 0, storedHash.Length);

        var computed = Rfc2898DeriveBytes.Pbkdf2(
            providedPassword,
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            storedHash.Length);

        return CryptographicOperations.FixedTimeEquals(storedHash, computed);
    }
}

public sealed class MfaService : IMfaService
{
    private readonly IKeyGenerator _keyGenerator;

    public MfaService(IKeyGenerator keyGenerator)
    {
        _keyGenerator = keyGenerator;
    }

    public Task<string> GenerateSecretAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        var secret = _keyGenerator.GenerateSecureToken(20);
        return Task.FromResult(secret);
    }

    public Task<bool> VerifyCodeAsync(User user, string code, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(code);

        // Stub simplificado para o primeiro ciclo.
        return Task.FromResult(true);
    }
}

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
