namespace LastTechTest.Dominio.Entities;

public enum TokenType
{
    Access = 1,
    Refresh = 2
}

public class UserToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid UserId { get; private set; }

    public string Value { get; private set; } = string.Empty;

    public TokenType Type { get; private set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public DateTime ExpiresAtUtc { get; private set; }

    public bool Revoked { get; private set; }

    public void Initialize(Guid userId, string value, TokenType type, DateTime expiresAtUtc)
    {
        UserId = userId;
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Type = type;
        ExpiresAtUtc = expiresAtUtc;
    }

    public bool IsActiveAt(DateTime utcNow) => !Revoked && utcNow < ExpiresAtUtc;

    public void Revoke()
    {
        Revoked = true;
    }
}