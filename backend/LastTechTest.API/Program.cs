using System.Security.Claims;
using System.Text;

using FluentValidation;

using LastTechTest.API;
using LastTechTest.Aplicacao.Anticipation.Commands.ApproveAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.CancelAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.ConvertSimulationToRealRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.CreateAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.RejectAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Commands.SimulateAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Queries.GetAnticipationRequestById;
using LastTechTest.Aplicacao.Anticipation.Queries.ListAnticipationRequests;
using LastTechTest.Aplicacao.Anticipation.Services;
using LastTechTest.Aplicacao.Anticipation.Simulation;
using LastTechTest.Aplicacao.Authentication.Commands.AdminCreateUser;
using LastTechTest.Aplicacao.Authentication.Commands.ChangePassword;
using LastTechTest.Aplicacao.Authentication.Commands.Login;
using LastTechTest.Aplicacao.Authentication.Commands.Logout;
using LastTechTest.Aplicacao.Authentication.Commands.RefreshToken;
using LastTechTest.Aplicacao.Authentication.Commands.RegisterUser;
using LastTechTest.Aplicacao.Authentication.Queries.GetLoggedUser;
using LastTechTest.Aplicacao.Common.Behaviors;
using LastTechTest.Aplicacao.Common.Exceptions;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Aplicacao.Common.Services;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.Services;
using LastTechTest.Infrastrutura;
using LastTechTest.Infrastrutura.Anticipation;
using LastTechTest.Persistencia;
using LastTechTest.Persistencia.Repositories;
using LastTechTest.Persistencia.Services;

using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Scalar.AspNetCore;

using Serilog;
using Serilog.Events;

// Keep in-memory connection alive for E2E tests (Testing environment).
SqliteConnection? testDbConnection = null;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

if (builder.Environment.IsEnvironment("Testing"))
{
    testDbConnection = new SqliteConnection("Data Source=:memory:");
    testDbConnection.Open();
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(testDbConnection));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                               "Data Source=lasttechtest.db";
        options.UseSqlite(connectionString);
    });
}

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<RegisterUserCommand>();
});
builder.Services.AddValidatorsFromAssemblyContaining<CreateAnticipationRequestCommand>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IMfaService, MfaService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IKeyGenerator, KeyGenerator>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
builder.Services.AddScoped<IAnticipationRequestRepository, AnticipationRequestRepository>();
builder.Services.AddScoped<IReceivableRepository, ReceivableRepository>();
builder.Services.AddScoped<IAnticipationCalculationService, AnticipationCalculationService>();
builder.Services.AddScoped<IEligibilityService, EligibilityService>();
builder.Services.AddScoped<IAnticipationAuditService, NoOpAnticipationAuditService>();
builder.Services.AddScoped<IAnticipationTransitionExecutor, AnticipationTransitionExecutor>();
builder.Services.AddSingleton<IAnticipationSimulationCache, MemoryAnticipationSimulationCache>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var secret = jwtSection["Secret"] ?? "change-me-in-production-super-secret-key";
var issuer = jwtSection["Issuer"] ?? "LastTechTest";
var audience = jwtSection["Audience"] ?? "LastTechTest-Users";

builder.Services
    .AddIdentityCore<IdentityUser<Guid>>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LastTechTest Auth API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Ensure schema exists: Migrate() for file DB, EnsureCreated() for E2E in-memory.
// Handles Docker volumes that were created with EnsureCreated() (no migration history).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DatabaseStartup.EnsureSchema(db, app.Environment.IsEnvironment("Testing"));

    // Seed Identity roles and admin user.
    IdentitySeeder.InitializeAsync(scope.ServiceProvider).GetAwaiter().GetResult();
}

if (app.Environment.IsDevelopment())
{
    app.MapSwagger("/openapi/{documentName}.json");
    app.MapScalarApiReference(options => options.WithTitle("LastTechTest Auth API"));
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

static IResult MapException(Exception ex)
{
    return ex switch
    {
        NotFoundException => Results.Json(new { error = ex.Message }, statusCode: 404),
        UnauthorizedAccessException => Results.Json(new { error = ex.Message }, statusCode: 403),
        InvalidOperationException => Results.BadRequest(new { error = ex.Message }),
        _ => Results.Json(new { error = "An error occurred." }, statusCode: 500)
    };
}

app.MapPost("/auth/register", async ([FromBody] RegisterUserCommand command, [FromServices] ISender sender, CancellationToken ct) =>
{
    try
    {
        var result = await sender.Send(command, ct);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return MapException(ex);
    }
});

app.MapPost("/auth/admin/users", async ([FromBody] AdminCreateUserCommand command, [FromServices] ISender sender, CancellationToken ct) =>
{
    try
    {
        var id = await sender.Send(command, ct);
        return Results.Created($"/auth/admin/users/{id}", new { Id = id });
    }
    catch (InvalidOperationException ex)
    {
        return MapException(ex);
    }
}).RequireAuthorization(policy => policy.RequireRole(LastTechTest.Dominio.Authorization.KnownRoles.Admin));

app.MapPost("/auth/login", async ([FromBody] LoginCommand command, [FromServices] ISender sender, CancellationToken ct) =>
{
    try
    {
        var result = await sender.Send(command, ct);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return MapException(ex);
    }
});

app.MapPost("/auth/change-password", async ([FromBody] ChangePasswordCommand command, [FromServices] ISender sender, CancellationToken ct) =>
{
    try
    {
        await sender.Send(command, ct);
        return Results.Ok();
    }
    catch (InvalidOperationException ex)
    {
        return MapException(ex);
    }
    catch (UnauthorizedAccessException ex)
    {
        return MapException(ex);
    }
}).RequireAuthorization();

app.MapPost("/auth/refresh", async ([FromBody] RefreshTokenCommand command, [FromServices] ISender sender, CancellationToken ct) =>
{
    try
    {
        var result = await sender.Send(command, ct);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return MapException(ex);
    }
});

app.MapDelete("/auth/logout", async ([FromBody] LogoutCommand command, [FromServices] ISender sender, CancellationToken ct) =>
{
    await sender.Send(command, ct);
    return Results.NoContent();
});

app.MapGet("/user/logged", async (ISender sender, CancellationToken ct) =>
{
    try
    {
        var result = await sender.Send(new GetLoggedUserQuery(), ct);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return MapException(ex);
    }
}).RequireAuthorization();

app.MapGet("/api/v1/anticipations", async (
    [FromQuery] Guid? creatorId,
    [FromQuery] int? status,
    [FromQuery] DateTime? fromUtc,
    [FromQuery] DateTime? toUtc,
    [FromQuery] int? page,
    [FromQuery] int? pageSize,
    [FromServices] ISender sender,
    CancellationToken ct) =>
{
    try
    {
        var query = new ListAnticipationRequestsQuery(
            creatorId,
            status,
            fromUtc,
            toUtc,
            page ?? 1,
            pageSize ?? 20);
        var result = await sender.Send(query, ct);
        return Results.Ok(new { result.Items, result.TotalCount });
    }
    catch (UnauthorizedAccessException ex)
    {
        return MapException(ex);
    }
}).RequireAuthorization(policy => policy.RequireRole(LastTechTest.Dominio.Authorization.KnownRoles.Creator, LastTechTest.Dominio.Authorization.KnownRoles.Admin));

app.MapGet("/api/v1/anticipations/{id:guid}", async (Guid id, [FromServices] ISender sender, CancellationToken ct) =>
{
    try
    {
        var result = await sender.Send(new GetAnticipationRequestByIdQuery(id), ct);
        if (result is null)
            return Results.NotFound();
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException ex)
    {
        return MapException(ex);
    }
}).RequireAuthorization(policy => policy.RequireRole(LastTechTest.Dominio.Authorization.KnownRoles.Creator, LastTechTest.Dominio.Authorization.KnownRoles.Admin));

// RA-4: Simulation (fake, no persistence) and convert to real (scaffolding: 501 until implemented)
app.MapPost("/api/v1/anticipations/simulations", async ([FromBody] SimulateAnticipationRequestDto? body, [FromServices] ISender sender, CancellationToken ct) =>
{
    if (body is null)
        return Results.BadRequest(new { error = "Request body is required." });
    try
    {
        var command = new SimulateAnticipationRequestCommand(body.RequestedAmount, body.CreatorId, body.RequestedAtUtc);
        var result = await sender.Send(command, ct);
        return Results.Ok(new { result.SimulationCode, ValidUntilUtc = result.ValidUntilUtc, result.RequestedAmount, result.GrossAmount, result.FeesAmount, result.NetAmount });
    }
    catch (NotImplementedException)
    {
        return Results.Json(new { error = "Not implemented." }, statusCode: 501);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (UnauthorizedAccessException ex)
    {
        return MapException(ex);
    }
    catch (InvalidOperationException ex)
    {
        return MapException(ex);
    }
}).RequireAuthorization();

app.MapPost("/api/v1/anticipations/simulations/{simulationCode}/confirm", async (string simulationCode, [FromServices] ISender sender, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(simulationCode))
        return Results.BadRequest(new { error = "Simulation code is required." });
    try
    {
        var result = await sender.Send(new ConvertSimulationToRealRequestCommand(simulationCode), ct);
        return Results.Created($"/api/v1/anticipations/{result.Id}", new { result.Id, result.Protocol, NetAmount = result.NetAmount, Status = result.Status.ToString() });
    }
    catch (NotImplementedException)
    {
        return Results.Json(new { error = "Not implemented." }, statusCode: 501);
    }
    catch (NotFoundException ex)
    {
        return MapException(ex);
    }
    catch (UnauthorizedAccessException ex)
    {
        return MapException(ex);
    }
    catch (InvalidOperationException ex)
    {
        var msg = ex.Message;
        if (msg.Contains("already used", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("open request", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("open anticipation", StringComparison.OrdinalIgnoreCase))
            return Results.Json(new { error = ex.Message }, statusCode: 422);
        return MapException(ex);
    }
}).RequireAuthorization();

app.MapPost("/api/v1/anticipations", async ([FromBody] CreateAnticipationRequestDto? body, [FromServices] ISender sender, CancellationToken ct) =>
{
    if (body is null)
        return Results.BadRequest(new { error = "Request body is required." });
    try
    {
        var command = new CreateAnticipationRequestCommand(
            body.RequestedAmount,
            body.CreatorId,
            body.RequestedAtUtc);
        var result = await sender.Send(command, ct);
        return Results.Created($"/api/v1/anticipations/{result.Id}", new { result.Id, result.Protocol, NetAmount = result.NetAmount, Status = result.Status.ToString() });
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (UnauthorizedAccessException ex)
    {
        return MapException(ex);
    }
    catch (InvalidOperationException ex)
    {
        return MapException(ex);
    }
}).RequireAuthorization();

// RA-3: approve / reject / cancel (stub until handlers implemented)
app.MapPost("/api/v1/anticipations/{id:guid}/approve", async (Guid id, [FromBody] ApproveAnticipationRequestDto? body, [FromServices] ISender sender, CancellationToken ct) =>
{
    try
    {
        var result = await sender.Send(new ApproveAnticipationRequestCommand(id, body?.Observation), ct);
        return Results.Ok(new { result.Id, result.Protocol, Status = result.Status.ToString() });
    }
    catch (NotImplementedException)
    {
        return Results.Json(new { error = "Not implemented." }, statusCode: 501);
    }
    catch (NotFoundException ex) { return MapException(ex); }
    catch (UnauthorizedAccessException ex) { return MapException(ex); }
    catch (InvalidOperationException ex) { return MapException(ex); }
}).RequireAuthorization(policy => policy.RequireRole(LastTechTest.Dominio.Authorization.KnownRoles.Analista, LastTechTest.Dominio.Authorization.KnownRoles.Admin));

app.MapPost("/api/v1/anticipations/{id:guid}/reject", async (Guid id, [FromBody] RejectAnticipationRequestDto? body, [FromServices] ISender sender, CancellationToken ct) =>
{
    if (body is null || string.IsNullOrWhiteSpace(body.Reason))
        return Results.BadRequest(new { error = "Reason is required." });
    try
    {
        var result = await sender.Send(new RejectAnticipationRequestCommand(id, body.Reason), ct);
        return Results.Ok(new { result.Id, result.Protocol, Status = result.Status.ToString() });
    }
    catch (NotImplementedException)
    {
        return Results.Json(new { error = "Not implemented." }, statusCode: 501);
    }
    catch (NotFoundException ex) { return MapException(ex); }
    catch (UnauthorizedAccessException ex) { return MapException(ex); }
    catch (InvalidOperationException ex) { return MapException(ex); }
}).RequireAuthorization(policy => policy.RequireRole(LastTechTest.Dominio.Authorization.KnownRoles.Analista, LastTechTest.Dominio.Authorization.KnownRoles.Admin));

app.MapPost("/api/v1/anticipations/{id:guid}/cancel", async (Guid id, [FromBody] CancelAnticipationRequestDto? body, [FromServices] ISender sender, CancellationToken ct) =>
{
    try
    {
        var result = await sender.Send(new CancelAnticipationRequestCommand(id, body?.Reason), ct);
        return Results.Ok(new { result.Id, result.Protocol, Status = result.Status.ToString(), result.AlreadyCanceled });
    }
    catch (NotImplementedException)
    {
        return Results.Json(new { error = "Not implemented." }, statusCode: 501);
    }
    catch (NotFoundException ex) { return MapException(ex); }
    catch (UnauthorizedAccessException ex) { return MapException(ex); }
    catch (InvalidOperationException ex) { return MapException(ex); }
}).RequireAuthorization(policy => policy.RequireRole(LastTechTest.Dominio.Authorization.KnownRoles.Creator, LastTechTest.Dominio.Authorization.KnownRoles.Admin));

app.Run();

/// <summary>Contrato de criação de solicitação de antecipação. Apenas 3 campos (padrão .NET PascalCase): RequestedAmount, CreatorId (opcional), RequestedAtUtc (opcional).</summary>
public sealed record CreateAnticipationRequestDto(decimal RequestedAmount, Guid? CreatorId, DateTime? RequestedAtUtc);

/// <summary>RA-3: body para POST approve. Observation opcional.</summary>
public sealed record ApproveAnticipationRequestDto(string? Observation);

/// <summary>RA-3: body para POST reject. Reason é obrigatório (validado no endpoint; ausência devolve 400 com "Reason is required.").</summary>
public sealed record RejectAnticipationRequestDto(string? Reason);

/// <summary>RA-3: body para POST cancel. Reason opcional.</summary>
public sealed record CancelAnticipationRequestDto(string? Reason);

/// <summary>RA-4: body para POST simulations. Same shape as create (RequestedAmount, CreatorId optional, RequestedAtUtc optional).</summary>
public sealed record SimulateAnticipationRequestDto(decimal RequestedAmount, Guid? CreatorId, DateTime? RequestedAtUtc);

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Claims is null) return null;
        var sub = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                  ?? user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    public string? GetRole()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Claims is null) return null;
        return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
    }
}