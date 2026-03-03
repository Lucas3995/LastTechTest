using FluentAssertions;
using LastTechTest.Dominio.Entities;
using LastTechTest.Persistencia;
using LastTechTest.Persistencia.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LastTechTest.Testes.Integration;

public class UserRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryIntegrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task Add_And_GetByEmail_Should_Work()
    {
        var user = new User();

        await _repository.AddAsync(user);

        var loaded = await _repository.GetByEmailAsync(user.Email);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(user.Id);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}

