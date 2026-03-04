using System.Security.Claims;

using FluentAssertions;

using LastTechTest.API.Services;
using LastTechTest.Aplicacao.Common.Interfaces;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class CurrentUserServiceTests
{
    [Fact]
    public void GetRole_Should_Read_From_ClaimsPrincipal()
    {
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        using var provider = services.BuildServiceProvider();

        var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        httpContextAccessor.HttpContext = context;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, "Admin")
        };

        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        var sut = provider.GetRequiredService<ICurrentUserService>();

        sut.GetRole().Should().Be("Admin");
    }
}