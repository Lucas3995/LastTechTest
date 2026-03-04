using FluentAssertions;

using LastTechTest.Aplicacao.Anticipation.Queries.GetAnticipationRequestById;
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
public class GetAnticipationRequestByIdHandlerIntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;

    public GetAnticipationRequestByIdHandlerIntegrationTests()
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

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetAnticipationRequestByIdQuery>());

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    /// <summary>Creator consulta id próprio → 200 e detalhes (CA2 happy path).</summary>
    [Fact]
    public async Task I1_GetById_AsCreator_OwnRequest_Should_ReturnDetails()
    {
        var creatorId = Guid.NewGuid();
        var entity = AnticipationRequest.Create(creatorId, 100m, 100m, 2m, 98m);
        await AddRequest(entity);

        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var query = new GetAnticipationRequestByIdQuery(entity.Id);

        var result = await sender.Send(query);

        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.CreatorId.Should().Be(creatorId);
        result.NetAmount.Should().Be(98m);
    }

    /// <summary>CA2 – Creator consulta id de outro → 403.</summary>
    [Fact]
    public async Task I2_GetById_AsCreator_OtherCreatorRequest_Should_ThrowUnauthorized()
    {
        var creatorA = Guid.NewGuid();
        var creatorB = Guid.NewGuid();
        var entity = AnticipationRequest.Create(creatorB, 100m, 100m, 2m, 98m);
        await AddRequest(entity);

        SetFakeUser(creatorA, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var query = new GetAnticipationRequestByIdQuery(entity.Id);

        var act = () => sender.Send(query);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*not allowed*");
    }

    /// <summary>CA4 – Admin consulta qualquer id → 200.</summary>
    [Fact]
    public async Task I3_GetById_AsAdmin_AnyRequest_Should_ReturnDetails()
    {
        var creatorId = Guid.NewGuid();
        var entity = AnticipationRequest.Create(creatorId, 200m, 200m, 4m, 196m);
        await AddRequest(entity);

        SetFakeUser(Guid.NewGuid(), "Admin");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var query = new GetAnticipationRequestByIdQuery(entity.Id);

        var result = await sender.Send(query);

        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.CreatorId.Should().Be(creatorId);
        result.NetAmount.Should().Be(196m);
    }

    /// <summary>Id inexistente → null (404 na API).</summary>
    [Fact]
    public async Task I4_GetById_WhenNotFound_Should_ReturnNull()
    {
        SetFakeUser(Guid.NewGuid(), "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var query = new GetAnticipationRequestByIdQuery(Guid.NewGuid());

        var result = await sender.Send(query);

        result.Should().BeNull();
    }

    [Fact]
    public async Task I5_GetById_WhenNotAuthenticated_Should_ThrowUnauthorized()
    {
        var entity = AnticipationRequest.Create(Guid.NewGuid(), 100m, 100m, 2m, 98m);
        await AddRequest(entity);

        SetFakeUser(null, null);
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var query = new GetAnticipationRequestByIdQuery(entity.Id);

        var act = () => sender.Send(query);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
    }

    private async Task AddRequest(AnticipationRequest request)
    {
        using var scope = _provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAnticipationRequestRepository>();
        await repo.AddAsync(request);
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
