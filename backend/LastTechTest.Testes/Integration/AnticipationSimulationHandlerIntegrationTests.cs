using FluentAssertions;

using FluentValidation;

using LastTechTest.Aplicacao.Anticipation.Commands.ConvertSimulationToRealRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.SimulateAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Simulation;
using LastTechTest.Aplicacao.Common.Behaviors;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.Services;
using LastTechTest.Persistencia;
using LastTechTest.Persistencia.Repositories;

using MediatR;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LastTechTest.Testes.Integration;

/// <summary>RA-4: Integration tests for simulation (no persistence) and conversion (persist with identical values). Uses InMemoryAnticipationSimulationCache.</summary>
[Trait("Category", "Integration")]
public class AnticipationSimulationHandlerIntegrationTests
{
    private readonly ServiceProvider _provider;
    private readonly InMemoryAnticipationSimulationCache _cache = new();

    public AnticipationSimulationHandlerIntegrationTests()
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
        services.AddSingleton<IAnticipationSimulationCache>(_cache);
        var fake = new FakeCurrentUserService();
        services.AddSingleton<FakeCurrentUserService>(fake);
        services.AddSingleton<ICurrentUserService>(fake);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SimulateAnticipationRequestCommand>());
        services.AddValidatorsFromAssemblyContaining<SimulateAnticipationRequestCommand>();
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    /// <summary>CA3: Simulation does not create any AnticipationRequest record.</summary>
    [Fact]
    public async Task I1_SimulateAnticipationRequest_DoesNotPersist_AnticipationRequestCountUnchanged()
    {
        var creatorId = Guid.NewGuid();
        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var repo = scope.ServiceProvider.GetRequiredService<IAnticipationRequestRepository>();
        var (_, countBefore) = await repo.ListAsync(creatorId, null, null, null, 1, 10);

        var result = await sender.Send(new SimulateAnticipationRequestCommand(1000m, null, null));

        result.Should().NotBeNull();
        result.SimulationCode.Should().NotBeNullOrWhiteSpace();
        var (_, countAfter) = await repo.ListAsync(creatorId, null, null, null, 1, 10);
        countAfter.Should().Be(countBefore);
    }

    /// <summary>CA6: Second simulation for same creator replaces first in cache.</summary>
    [Fact]
    public async Task I2_SimulateTwiceSameCreator_SecondReplacesFirst_InCache()
    {
        var creatorId = Guid.NewGuid();
        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var first = await sender.Send(new SimulateAnticipationRequestCommand(100m, null, null));
        var second = await sender.Send(new SimulateAnticipationRequestCommand(200m, null, null));

        first.SimulationCode.Should().NotBe(second.SimulationCode);
        var firstEntry = await _cache.GetAsync(first.SimulationCode);
        firstEntry.Should().BeNull();
        var secondEntry = await _cache.GetAsync(second.SimulationCode);
        secondEntry.Should().NotBeNull();
        secondEntry!.Data.RequestedAmount.Should().Be(200m);
    }

    /// <summary>CA2: Invalid params (e.g. amount &lt; 100) — validation fails, no persistence.</summary>
    [Fact]
    public async Task I3_SimulateAnticipationRequest_InvalidAmount_FailsValidation_NoPersist()
    {
        var creatorId = Guid.NewGuid();
        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var repo = scope.ServiceProvider.GetRequiredService<IAnticipationRequestRepository>();
        var (_, countBefore) = await repo.ListAsync(creatorId, null, null, null, 1, 10);

        var act = () => sender.Send(new SimulateAnticipationRequestCommand(50m, null, null));

        await act.Should().ThrowAsync<ValidationException>();
        var (_, countAfter) = await repo.ListAsync(creatorId, null, null, null, 1, 10);
        countAfter.Should().Be(countBefore);
    }

    /// <summary>CA7: Conversion with valid cache entry and no pending request creates one AnticipationRequest with identical values.</summary>
    [Fact]
    public async Task I4_ConvertSimulationToRealRequest_ValidCode_NoPending_CreatesRequestWithIdenticalValues()
    {
        var creatorId = Guid.NewGuid();
        var requestedAt = DateTime.UtcNow.AddMinutes(-10);
        var data = new SimulationData(creatorId, 500m, 500m, 25m, 475m, requestedAt);
        var code = await _cache.StoreAsync(creatorId, data, TimeSpan.FromHours(2));

        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var repo = scope.ServiceProvider.GetRequiredService<IAnticipationRequestRepository>();

        var result = await sender.Send(new ConvertSimulationToRealRequestCommand(code));

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.NetAmount.Should().Be(475m);
        var persisted = await repo.GetByIdAsync(result.Id);
        persisted.Should().NotBeNull();
        persisted!.RequestedAmount.Should().Be(500m);
        persisted.GrossAmount.Should().Be(500m);
        persisted.FeesAmount.Should().Be(25m);
        persisted.NetAmount.Should().Be(475m);
        persisted.RequestedAtUtc.Should().Be(requestedAt);
    }

    /// <summary>CA8: Second conversion with same code fails (simulation already used).</summary>
    [Fact]
    public async Task I5_ConvertSimulationToRealRequest_SameCodeTwice_SecondFails()
    {
        var creatorId = Guid.NewGuid();
        var data = new SimulationData(creatorId, 300m, 300m, 15m, 285m, DateTime.UtcNow);
        var code = await _cache.StoreAsync(creatorId, data, TimeSpan.FromHours(2));

        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var first = await sender.Send(new ConvertSimulationToRealRequestCommand(code));
        first.Should().NotBeNull();

        var act = () => sender.Send(new ConvertSimulationToRealRequestCommand(code));
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already used*");
    }

    /// <summary>CA8: Conversion when creator already has open request fails.</summary>
    [Fact]
    public async Task I6_ConvertSimulationToRealRequest_WhenCreatorHasPendingRequest_Fails()
    {
        var creatorId = Guid.NewGuid();
        var data = new SimulationData(creatorId, 200m, 200m, 10m, 190m, DateTime.UtcNow);
        var code = await _cache.StoreAsync(creatorId, data, TimeSpan.FromHours(2));

        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var repo = scope.ServiceProvider.GetRequiredService<IAnticipationRequestRepository>();

        var existing = AnticipationRequest.Create(creatorId, 100m, 100m, 5m, 95m, DateTime.UtcNow);
        await repo.AddAsync(existing);

        var act = () => sender.Send(new ConvertSimulationToRealRequestCommand(code));
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*open*");
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
