using System.Security.Cryptography;

using Microsoft.AspNetCore.WebUtilities;

namespace LastTechTest.Infrastrutura;

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