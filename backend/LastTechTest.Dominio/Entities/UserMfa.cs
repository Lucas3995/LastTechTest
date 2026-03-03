namespace LastTechTest.Dominio.Entities;

public class UserMfa
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid UserId { get; private set; }

    public string SecretKey { get; private set; } = string.Empty;

    public bool Enabled { get; private set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public void Enable(Guid userId, string secretKey)
    {
        UserId = userId;
        SecretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
        Enabled = true;
    }

    public void Disable()
    {
        Enabled = false;
    }
}