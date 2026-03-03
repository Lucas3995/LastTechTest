using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LastTechTest.Persistencia.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var tracked = await _context.Users.FindAsync(new object[] { user.Id }, cancellationToken);
        if (tracked is not null)
        {
            _context.Entry(tracked).CurrentValues.SetValues(user);
        }
        else
        {
            _context.Users.Update(user);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}

