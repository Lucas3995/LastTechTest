using FluentAssertions;

using LastTechTest.Infrastrutura;

using Microsoft.Extensions.Configuration;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class SmtpEmailServiceTests
{
    [Fact]
    public async Task SendAsync_Completes_Without_Throwing()
    {
        var config = new ConfigurationBuilder().Build();
        var sut = new SmtpEmailService(config);

        var act = () => sut.SendAsync("a@b.com", "Subj", "Body", CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}