using FluentAssertions;

using LastTechTest.Dominio.Entities;
using LastTechTest.Persistencia;
using LastTechTest.Persistencia.Repositories;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LastTechTest.Testes.Integration;

[Trait("Category", "Integration")]
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

    [Fact]
    public async Task RF6_T2_ListAsync_ShouldReturnUsers_WithBasicConsistency()
    {
        var first = new User();
        first.SetEmail($"rf6-first-{Guid.NewGuid():N}@example.com");
        first.SetPasswordHash("hash-1");
        await _repository.AddAsync(first);

        var second = new User();
        second.SetEmail($"rf6-second-{Guid.NewGuid():N}@example.com");
        second.SetPasswordHash("hash-2");
        await _repository.AddAsync(second);

        var listMethod = typeof(UserRepository).GetMethod("ListAsync", new[] { typeof(CancellationToken) });
        listMethod.Should().NotBeNull("RF-6 requires UserRepository.ListAsync(CancellationToken)");

        var task = listMethod!.Invoke(_repository, new object[] { CancellationToken.None }) as Task;
        task.Should().NotBeNull();
        await task!;

        var resultObj = task.GetType().GetProperty("Result")?.GetValue(task);
        resultObj.Should().NotBeNull();

        var rows = ((System.Collections.IEnumerable)resultObj!).Cast<object>().ToList();
        rows.Should().NotBeEmpty();
        rows.Select(x => x.GetType().GetProperty("Email")?.GetValue(x)?.ToString())
            .Should()
            .Contain(first.Email)
            .And.Contain(second.Email);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
