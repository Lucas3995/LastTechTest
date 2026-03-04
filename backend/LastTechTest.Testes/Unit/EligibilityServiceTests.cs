using FluentAssertions;

using LastTechTest.Dominio.Services;
using LastTechTest.Dominio.ValueObjects;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class EligibilityServiceTests
{
    private readonly EligibilityService _sut = new();

    [Fact]
    public void U3_EligibilityService_ReceivableNotEligible_Should_BeExcluded()
    {
        var pastDue = new ReceivableInfo(Guid.NewGuid(), 100m, DateTime.UtcNow.AddDays(-1), ReceivableStatus.Eligible);
        var ineligibleStatus = new ReceivableInfo(Guid.NewGuid(), 100m, DateTime.UtcNow.AddDays(5), ReceivableStatus.Ineligible);

        _sut.IsEligible(pastDue).Should().BeFalse();
        _sut.IsEligible(ineligibleStatus).Should().BeFalse();
    }

    [Fact]
    public void U4_EligibilityService_AllEligible_Should_ReturnOnlyEligibleReceivables()
    {
        var eligible = new ReceivableInfo(Guid.NewGuid(), 100m, DateTime.UtcNow.AddDays(10), ReceivableStatus.Eligible);
        var ineligible = new ReceivableInfo(Guid.NewGuid(), 50m, DateTime.UtcNow.AddDays(5), ReceivableStatus.Paid);
        var receivables = new[] { eligible, ineligible };

        var filtered = _sut.FilterEligible(receivables);

        filtered.Should().ContainSingle().Which.Id.Should().Be(eligible.Id);
    }
}
