using FluentAssertions;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class IdentityPasswordOptionsTests
{
    [Fact]
    public void Identity_Should_Have_Configured_Password_Policy()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services
            .AddIdentityCore<IdentityUser<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            });

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<IdentityOptions>>().Value;

        options.Password.RequiredLength.Should().Be(8);
        options.Password.RequireDigit.Should().BeTrue();
        options.Password.RequireLowercase.Should().BeTrue();
        options.Password.RequireUppercase.Should().BeTrue();
        options.Password.RequireNonAlphanumeric.Should().BeTrue();
    }
}