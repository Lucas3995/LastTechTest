using System.Security.Cryptography;

using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

namespace LastTechTest.Infrastrutura;

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
