using FluentAssertions;

using LastTechTest.Aplicacao.Anticipation.Commands.CreateAnticipationRequest;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.Services;
using LastTechTest.Dominio.ValueObjects;
using LastTechTest.Persistencia;
using LastTechTest.Persistencia.Repositories;

using MediatR;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LastTechTest.Testes.Integration;

[Trait("Category", "Integration")]
public class CreateAnticipationRequestHandlerIntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;

    public CreateAnticipationRequestHandlerIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));

        services.AddScoped<IAnticipationRequestRepository, AnticipationRequestRepository>();
        services.AddScoped<IReceivableRepository, ReceivableRepository>();
        services.AddScoped<IAnticipationCalculationService, AnticipationCalculationService>();
        services.AddScoped<IEligibilityService, EligibilityService>();
        var fake = new FakeCurrentUserService();
        services.AddSingleton<FakeCurrentUserService>(fake);
        services.AddSingleton<ICurrentUserService>(fake);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateAnticipationRequestCommand>());

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task I1_CreateAnticipationRequestHandler_ValidCommand_AsCreator_Should_PersistAndReturnId()
    {
        var creatorId = Guid.NewGuid();
        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new CreateAnticipationRequestCommand(1000m, CreatorId: null);

        var response = await sender.Send(command);

        response.Should().NotBeNull();
        response.Id.Should().NotBeEmpty();
        response.Protocol.Should().NotBeNullOrWhiteSpace();
        response.NetAmount.Should().BeGreaterThan(0);
        response.Status.Should().BeOneOf(Dominio.Enums.AnticipationRequestStatus.Created, Dominio.Enums.AnticipationRequestStatus.Pending);
        var repo = scope.ServiceProvider.GetRequiredService<IAnticipationRequestRepository>();
        var persisted = await repo.GetByIdAsync(response.Id);
        persisted.Should().NotBeNull();
        persisted!.CreatorId.Should().Be(creatorId);
    }

    [Fact]
    public async Task I2_CreateAnticipationRequestHandler_ValidCommand_AsAdmin_WithCreatorId_Should_PersistForThatCreator()
    {
        var adminUserId = Guid.NewGuid();
        var targetCreatorId = Guid.NewGuid();
        SetFakeUser(adminUserId, "Admin");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new CreateAnticipationRequestCommand(1000m, CreatorId: targetCreatorId);

        var response = await sender.Send(command);

        response.Should().NotBeNull();
        response.NetAmount.Should().BeGreaterThan(0);
        response.Status.Should().BeOneOf(Dominio.Enums.AnticipationRequestStatus.Created, Dominio.Enums.AnticipationRequestStatus.Pending);
        var repo = scope.ServiceProvider.GetRequiredService<IAnticipationRequestRepository>();
        var persisted = await repo.GetByIdAsync(response.Id);
        persisted.Should().NotBeNull();
        persisted!.CreatorId.Should().Be(targetCreatorId);
    }

    [Fact]
    public async Task I3_CreateAnticipationRequestHandler_CreatorTryingOtherCreatorId_Should_Reject()
    {
        var creatorId = Guid.NewGuid();
        var otherCreatorId = Guid.NewGuid();
        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new CreateAnticipationRequestCommand(1000m, CreatorId: otherCreatorId);

        var act = () => sender.Send(command);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task I4_CreateAnticipationRequestHandler_AboveLimit_Should_ReturnValidationError()
    {
        SetFakeUser(Guid.NewGuid(), "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new CreateAnticipationRequestCommand(50_000m);

        var act = () => sender.Send(command);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*limit*");
    }

    [Fact]
    public async Task I5_CreateAnticipationRequestHandler_IneligibleReceivables_Should_ReturnValidationError()
    {
        var receivableId = Guid.NewGuid();
        var services = new ServiceCollection();
        services.AddLogging();
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        services.AddScoped<IAnticipationRequestRepository, AnticipationRequestRepository>();
        services.AddScoped<IReceivableRepository>(_ => new IneligibleReceivablesStub(receivableId));
        services.AddScoped<IAnticipationCalculationService, AnticipationCalculationService>();
        services.AddScoped<IEligibilityService, EligibilityService>();
        var fake = new FakeCurrentUserService();
        fake.Set(Guid.NewGuid(), "Creator");
        services.AddSingleton<FakeCurrentUserService>(fake);
        services.AddSingleton<ICurrentUserService>(fake);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateAnticipationRequestCommand>());
        var provider = services.BuildServiceProvider();
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        }
        using var scope2 = provider.CreateScope();
        var sender = scope2.ServiceProvider.GetRequiredService<ISender>();
        var command = new CreateAnticipationRequestCommand(1000m);

        var act = () => sender.Send(command);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*eligible*");
    }

    private sealed class IneligibleReceivablesStub : IReceivableRepository
    {
        private readonly Guid _id;

        public IneligibleReceivablesStub(Guid id) => _id = id;

        public Task<IReadOnlyList<ReceivableInfo>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var list = ids.Select(id => new ReceivableInfo(id, 100m, DateTime.UtcNow.AddDays(-1), ReceivableStatus.Paid)).ToList();
            return Task.FromResult<IReadOnlyList<ReceivableInfo>>(list);
        }

        public Task<IReadOnlyList<ReceivableInfo>> GetEligibleByCreatorIdAsync(Guid creatorId, CancellationToken cancellationToken = default)
        {
            _ = creatorId;
            _ = cancellationToken;
            return Task.FromResult<IReadOnlyList<ReceivableInfo>>(new List<ReceivableInfo> { new(_id, 100m, DateTime.UtcNow.AddDays(-1), ReceivableStatus.Paid) });
        }
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
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