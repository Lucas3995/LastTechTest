using System.Security.Claims;
using System.Text;

using FluentValidation;

using LastTechTest.API;
using LastTechTest.API.Configuration;
using LastTechTest.Aplicacao.Anticipation.Commands.CreateAnticipationRequest;
using LastTechTest.Aplicacao.Anticipation.Services;
using LastTechTest.Aplicacao.Anticipation.Simulation;
using LastTechTest.Aplicacao.Authentication.Commands.RegisterUser;
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
builder.Services.AddScoped<ICurrentUserService, LastTechTest.API.Services.CurrentUserService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserTokenRepository, UserTokenRepository>();
builder.Services.AddScoped<IAnticipationRequestRepository, AnticipationRequestRepository>();
builder.Services.AddScoped<IReceivableRepository, ReceivableRepository>();
builder.Services.Configure<AnticipationCalculationOptions>(builder.Configuration.GetSection(AnticipationCalculationOptions.SectionName));
builder.Services.AddSingleton<IAnticipationCalculationSettings, LastTechTest.API.Services.AnticipationCalculationSettingsAdapter>();
builder.Services.AddScoped<IAnticipationCalculationService, AnticipationCalculationService>();
builder.Services.AddScoped<IEligibilityService, EligibilityService>();
if (builder.Environment.IsEnvironment("Testing") || builder.Environment.IsDevelopment())
    builder.Services.AddScoped<IAnticipationAuditService, NoOpAnticipationAuditService>();
else
    builder.Services.AddScoped<IAnticipationAuditService, AnticipationAuditService>();
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

builder.Services.AddControllers();
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

app.MapControllers();

app.Run();