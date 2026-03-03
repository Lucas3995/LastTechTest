using System.Security.Claims;
using System.Text;

using LastTechTest.Aplicacao.Authentication.Commands.Login;
using LastTechTest.Aplicacao.Authentication.Commands.Logout;
using LastTechTest.Aplicacao.Authentication.Commands.RefreshToken;
using LastTechTest.Aplicacao.Authentication.Commands.RegisterUser;
using LastTechTest.Aplicacao.Authentication.Queries.GetLoggedUser;
using LastTechTest.Aplicacao.Common.Interfaces;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Infrastrutura;
using LastTechTest.Persistencia;
using LastTechTest.Persistencia.Repositories;

using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Scalar.AspNetCore;

using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                           "Data Source=lasttechtest.db";
    options.UseSqlite(connectionString);
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<RegisterUserCommand>();
});

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IMfaService, MfaService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IKeyGenerator, KeyGenerator>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var secret = jwtSection["Secret"] ?? "change-me-in-production-super-secret-key";
var issuer = jwtSection["Issuer"] ?? "LastTechTest";
var audience = jwtSection["Audience"] ?? "LastTechTest-Users";

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

// Ensure database schema exists on startup (simplified for SQLite placeholder).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
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
    return ex is InvalidOperationException
        ? Results.BadRequest(new { error = ex.Message })
        : Results.Json(new { error = "An error occurred." }, statusCode: 500);
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

app.Run();

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
}