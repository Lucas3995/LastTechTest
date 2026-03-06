namespace LastTechTest.Dominio.Entities;

public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserStatus Status { get; private set; } = UserStatus.Active;

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public DateTime? LastLoginAtUtc { get; private set; }

    public void SetEmail(string email)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
    }

    public void MarkLoggedIn(DateTime loggedInAtUtc)
    {
        LastLoginAtUtc = loggedInAtUtc;
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
    }
}
