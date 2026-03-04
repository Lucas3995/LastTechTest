using FluentAssertions;

using LastTechTest.Aplicacao.Anticipation.Commands.ApproveAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.CancelAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.CreateAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.RejectAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Services;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Aplicacao.Common.Services;
using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Enums;
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

/// <summary>RA-3 integration tests: Approve, Reject, Cancel handlers (persistence, audit, idempotence, permissions).</summary>
[Trait("Category", "Integration")]
public class AnticipationTransitionHandlersIntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;

    public AnticipationTransitionHandlersIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));

        services.AddScoped<IAnticipationRequestRepository, AnticipationRequestRepository>();
        services.AddScoped<IAnticipationAuditService, NoOpAnticipationAuditService>();
        services.AddScoped<IAnticipationTransitionExecutor, AnticipationTransitionExecutor>();
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
    public async Task Approve_WhenRequestInPending_AsAnalista_Should_InvokeHandler()
    {
        var (requestId, _) = await SeedRequestInPendingAsync(Guid.NewGuid());
        SetFakeUser(Guid.NewGuid(), "Analista");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new ApproveAnticipationRequestCommand(requestId, "Aprovado conforme política.");

        var result = await sender.Send(command);

        result.Should().NotBeNull();
        result.Status.Should().Be(AnticipationRequestStatus.Approved);
        result.Id.Should().Be(requestId);
        var repo = scope.ServiceProvider.GetRequiredService<IAnticipationRequestRepository>();
        var loaded = await repo.GetByIdAsync(requestId);
        loaded.Should().NotBeNull();
        loaded!.Status.Should().Be(AnticipationRequestStatus.Approved);
    }

    [Fact]
    public async Task Approve_WhenRequestInPending_AsAdmin_Should_InvokeHandler()
    {
        var (requestId, _) = await SeedRequestInPendingAsync(Guid.NewGuid());
        SetFakeUser(Guid.NewGuid(), "Admin");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new ApproveAnticipationRequestCommand(requestId, "Aprovado por admin.");

        var result = await sender.Send(command);

        result.Status.Should().Be(AnticipationRequestStatus.Approved);
        var repo = scope.ServiceProvider.GetRequiredService<IAnticipationRequestRepository>();
        var loaded = await repo.GetByIdAsync(requestId);
        loaded!.Status.Should().Be(AnticipationRequestStatus.Approved);
    }

    [Fact]
    public async Task Reject_WhenRequestInPending_AsAnalista_WithReason_Should_InvokeHandler()
    {
        var (requestId, _) = await SeedRequestInPendingAsync(Guid.NewGuid());
        SetFakeUser(Guid.NewGuid(), "Analista");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new RejectAnticipationRequestCommand(requestId, "Documentação insuficiente.");

        var result = await sender.Send(command);

        result.Status.Should().Be(AnticipationRequestStatus.Rejected);
        var repo = scope.ServiceProvider.GetRequiredService<IAnticipationRequestRepository>();
        var loaded = await repo.GetByIdAsync(requestId);
        loaded!.Status.Should().Be(AnticipationRequestStatus.Rejected);
    }

    [Fact]
    public async Task Cancel_WhenRequestInPending_AsCreatorOwner_Should_InvokeHandler()
    {
        var creatorId = Guid.NewGuid();
        var (requestId, _) = await SeedRequestInPendingAsync(creatorId);
        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new CancelAnticipationRequestCommand(requestId, "Desistência.");

        var result = await sender.Send(command);

        result.Status.Should().Be(AnticipationRequestStatus.CanceledByCreator);
        result.AlreadyCanceled.Should().BeFalse();
        var repo = scope.ServiceProvider.GetRequiredService<IAnticipationRequestRepository>();
        var loaded = await repo.GetByIdAsync(requestId);
        loaded!.Status.Should().Be(AnticipationRequestStatus.CanceledByCreator);
    }

    [Fact]
    public async Task Cancel_WhenRequestInPending_AsAdmin_Should_InvokeHandler()
    {
        var (requestId, _) = await SeedRequestInPendingAsync(Guid.NewGuid());
        SetFakeUser(Guid.NewGuid(), "Admin");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new CancelAnticipationRequestCommand(requestId);

        var result = await sender.Send(command);

        result.Status.Should().Be(AnticipationRequestStatus.CanceledByCreator);
        result.AlreadyCanceled.Should().BeFalse();
    }

    [Fact]
    public async Task Approve_WhenRequestInPending_AsCreator_Should_BeDeniedByHandler()
    {
        var creatorId = Guid.NewGuid();
        var (requestId, _) = await SeedRequestInPendingAsync(creatorId);
        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new ApproveAnticipationRequestCommand(requestId);

        var act = () => sender.Send(command);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Reject_WhenRequestInPending_AsCreator_Should_BeDeniedByHandler()
    {
        var creatorId = Guid.NewGuid();
        var (requestId, _) = await SeedRequestInPendingAsync(creatorId);
        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new RejectAnticipationRequestCommand(requestId, "Motivo.");

        var act = () => sender.Send(command);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Cancel_WhenRequestAlreadyCanceled_Should_ReturnIdempotentResponse()
    {
        var creatorId = Guid.NewGuid();
        var (requestId, _) = await SeedRequestInStatusAsync(creatorId, AnticipationRequestStatus.CanceledByCreator);
        SetFakeUser(creatorId, "Creator");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new CancelAnticipationRequestCommand(requestId);

        var result = await sender.Send(command);

        result.AlreadyCanceled.Should().BeTrue();
        result.Status.Should().Be(AnticipationRequestStatus.CanceledByCreator);
    }

    [Fact]
    public async Task Approve_WhenRequestAlreadyApproved_Should_RejectTransition()
    {
        var (requestId, _) = await SeedRequestInStatusAsync(Guid.NewGuid(), AnticipationRequestStatus.Approved);
        SetFakeUser(Guid.NewGuid(), "Analista");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new ApproveAnticipationRequestCommand(requestId, "Obs.");

        var act = () => sender.Send(command);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Reject_WhenRequestAlreadyRejected_Should_RejectTransition()
    {
        var (requestId, _) = await SeedRequestInStatusAsync(Guid.NewGuid(), AnticipationRequestStatus.Rejected);
        SetFakeUser(Guid.NewGuid(), "Analista");
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new RejectAnticipationRequestCommand(requestId, "Motivo.");

        var act = () => sender.Send(command);

        await act.Should().ThrowAsync<InvalidOperationException>();
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

    private async Task<(Guid RequestId, Guid CreatorId)> SeedRequestInPendingAsync(Guid creatorId)
    {
        return await SeedRequestInStatusAsync(creatorId, AnticipationRequestStatus.Pending);
    }

    private async Task<(Guid RequestId, Guid CreatorId)> SeedRequestInStatusAsync(Guid creatorId, AnticipationRequestStatus status)
    {
        var entity = AnticipationRequest.Create(creatorId, 1000m, 1000m, 20m, 980m);
        entity.SetStatusForTest(status);

        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.AnticipationRequests.Add(entity);
        await db.SaveChangesAsync();
        return (entity.Id, creatorId);
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