using FluentAssertions;

using LastTechTest.Aplicacao.Anticipation.Queries.ListAnticipationRequests;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Persistencia;
using LastTechTest.Persistencia.Repositories;

using MediatR;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LastTechTest.Testes.Integration;

[Trait("Category", "Integration")]
public class ListAnticipationRequestsHandlerIntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;

    public ListAnticipationRequestsHandlerIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));

        services.AddScoped<IAnticipationRequestRepository, AnticipationRequestRepository>();
        var fake = new FakeCurrentUserService();
        services.AddSingleton<FakeCurrentUserService>(fake);
        services.AddSingleton<ICurrentUserService>(fake);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ListAnticipationRequestsQuery>());

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    /// <summary>CA1 – Creator vê apenas suas próprias solicitações.</summary>
    [Fact]
    public async Task I1_List_AsCreator_Should_ReturnOnlyOwnRequests()
    {
        var creatorA = Guid.NewGuid();
        var creatorB = Guid.NewGuid();
        await SeedRequests(creatorA, 2);
        await SeedRequests(creatorB, 1);

        SetFakeUser(creatorA, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var query = new ListAnticipationRequestsQuery(null, null, null, null, 1, 20);

        var result = await sender.Send(query);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(x => x.CreatorId == creatorA);
    }

    /// <summary>CA3 – Admin sem filtro vê todas as solicitações.</summary>
    [Fact]
    public async Task I2_List_AsAdmin_NoFilter_Should_ReturnAllRequests()
    {
        var creatorA = Guid.NewGuid();
        var creatorB = Guid.NewGuid();
        await SeedRequests(creatorA, 1);
        await SeedRequests(creatorB, 2);

        SetFakeUser(Guid.NewGuid(), "Admin");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var query = new ListAnticipationRequestsQuery(null, null, null, null, 1, 20);

        var result = await sender.Send(query);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }

    /// <summary>CA3 – Admin com filtro por creator_id vê só desse creator.</summary>
    [Fact]
    public async Task I3_List_AsAdmin_WithCreatorFilter_Should_ReturnOnlyThatCreatorRequests()
    {
        var creatorA = Guid.NewGuid();
        var creatorB = Guid.NewGuid();
        await SeedRequests(creatorA, 2);
        await SeedRequests(creatorB, 1);

        SetFakeUser(Guid.NewGuid(), "Admin");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var query = new ListAnticipationRequestsQuery(creatorB, null, null, null, 1, 20);

        var result = await sender.Send(query);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].CreatorId.Should().Be(creatorB);
    }

    /// <summary>CA5 – Paginação: page e pageSize aplicados, TotalCount correto.</summary>
    [Fact]
    public async Task I4_List_WithPagination_Should_ReturnPageAndTotalCount()
    {
        var creatorId = Guid.NewGuid();
        await SeedRequests(creatorId, 5);

        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var query = new ListAnticipationRequestsQuery(null, null, null, null, Page: 1, PageSize: 2);

        var result = await sender.Send(query);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task I5_List_WhenNotAuthenticated_Should_ThrowUnauthorized()
    {
        SetFakeUser(null, null);
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var query = new ListAnticipationRequestsQuery(null, null, null, null, 1, 20);

        var act = () => sender.Send(query);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
    }

    private async Task SeedRequests(Guid creatorId, int count)
    {
        using var scope = _provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAnticipationRequestRepository>();
        for (var i = 0; i < count; i++)
        {
            var entity = AnticipationRequest.Create(creatorId, 100m * (i + 1), 100m * (i + 1), 2m, 98m * (i + 1));
            await repo.AddAsync(entity);
        }
    }

    private void SetFakeUser(Guid? userId, string? role)
    {
        _provider.GetRequiredService<FakeCurrentUserService>().Set(userId, role);
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        private Guid? _userId;
        private string? _role;

        public Guid? GetCurrentUserId() => _userId;
        public string? GetRole() => _role;

        public void Set(Guid? userId, string? role)
        {
            _userId = userId;
            _role = role;
        }
    }
}
