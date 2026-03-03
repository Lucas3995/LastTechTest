using LastTechTest.Dominio.Entities;

namespace LastTechTest.Dominio.Interfaces;

public interface IUserPasswordHasher
{
    string HashPassword(User user, string password);

    bool VerifyHashedPassword(User user, string hashedPassword, string providedPassword);
}

