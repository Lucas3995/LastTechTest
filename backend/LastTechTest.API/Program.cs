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
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/auth/register", async (RegisterUserCommand command, ISender sender, CancellationToken ct) =>
{
    var result = await sender.Send(command, ct);
    return Results.Ok(result);
});

app.MapPost("/auth/login", async (LoginCommand command, ISender sender, CancellationToken ct) =>
{
    var result = await sender.Send(command, ct);
    return Results.Ok(result);
});

app.MapPost("/auth/refresh", async (RefreshTokenCommand command, ISender sender, CancellationToken ct) =>
{
    var result = await sender.Send(command, ct);
    return Results.Ok(result);
});

app.MapDelete("/auth/logout", async (LogoutCommand command, ISender sender, CancellationToken ct) =>
{
    await sender.Send(command, ct);
    return Results.NoContent();
});

app.MapGet("/user/logged", async (ISender sender, CancellationToken ct) =>
{
    var result = await sender.Send(new GetLoggedUserQuery(), ct);
    return Results.Ok(result);
}).RequireAuthorization();

app.Run();

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var sub = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? user?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }
}
