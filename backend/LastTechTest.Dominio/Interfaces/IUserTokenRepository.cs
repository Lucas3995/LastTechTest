using LastTechTest.Dominio.Entities;

namespace LastTechTest.Dominio.Interfaces;

public interface IUserTokenRepository
{
    Task<UserToken?> GetByValueAsync(string value, CancellationToken cancellationToken = default);

    Task AddAsync(UserToken token, CancellationToken cancellationToken = default);

    Task UpdateAsync(UserToken token, CancellationToken cancellationToken = default);
}