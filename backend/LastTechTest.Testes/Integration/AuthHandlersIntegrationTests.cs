using FluentAssertions;
using LastTechTest.Aplicacao.Authentication.Commands.Login;
using LastTechTest.Aplicacao.Authentication.Commands.RegisterUser;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Infrastrutura;
using LastTechTest.Persistencia;
using LastTechTest.Persistencia.Repositories;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LastTechTest.Testes.Integration;

public class AuthHandlersIntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _provider;

    public AuthHandlersIntegrationTests()
    {
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "integration-test-secret-key-should-be-long-enough",
            ["Jwt:Issuer"] = "IntegrationTests",
            ["Jwt:Audience"] = "IntegrationTests-Audience"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        services.AddSingleton(configuration);

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connection));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserTokenRepository, UserTokenRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserPasswordHasher, PasswordHasher>();
        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IKeyGenerator, KeyGenerator>();
        services.AddScoped<ICurrentUserService, FakeCurrentUserService>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<RegisterUserCommand>();
        });

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task Register_Then_Login_Should_Return_Tokens()
    {
        using var scope = _provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var email = $"user-{Guid.NewGuid():N}@example.com";
        var password = "StrongPassword123!";

        var registerResult = await sender.Send(new RegisterUserCommand(email, password));
        registerResult.AccessToken.Should().NotBeNullOrWhiteSpace();

        var loginResult = await sender.Send(new LoginCommand(email, password));
        loginResult.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public Guid? GetCurrentUserId() => null;
    }
}

