using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace LastTechTest.Persistencia.Repositories;

public class UserTokenRepository : IUserTokenRepository
{
    private readonly ApplicationDbContext _context;

    public UserTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserToken?> GetByValueAsync(string value, CancellationToken cancellationToken = default)
    {
        return await _context.UserTokens
            .FirstOrDefaultAsync(x => x.Value == value, cancellationToken);
    }

    public async Task AddAsync(UserToken token, CancellationToken cancellationToken = default)
    {
        await _context.UserTokens.AddAsync(token, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserToken token, CancellationToken cancellationToken = default)
    {
        _context.UserTokens.Update(token);
        await _context.SaveChangesAsync(cancellationToken);
    }
}