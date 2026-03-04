using LastTechTest.Dominio.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LastTechTest.Persistencia;

public class ApplicationDbContext
    : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public new DbSet<User> Users => Set<User>();

    public new DbSet<UserToken> UserTokens => Set<UserToken>();

    public DbSet<UserMfa> UserMfas => Set<UserMfa>();

    public DbSet<AnticipationRequest> AnticipationRequests => Set<AnticipationRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}