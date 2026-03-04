using FluentAssertions;

using LastTechTest.Dominio.Entities;
using LastTechTest.Dominio.Enums;
using LastTechTest.Dominio.Interfaces;
using LastTechTest.Dominio.Services;
using LastTechTest.Dominio.ValueObjects;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class AnticipationRequestEntityTests
{
    [Fact]
    public void U7_AnticipationRequest_InitialState_Should_BeCreatedOrPending()
    {
        var entity = AnticipationRequest.Create(Guid.NewGuid(), 100m, 100m, 2m, 98m);

        entity.Status.Should().BeOneOf(AnticipationRequestStatus.Created, AnticipationRequestStatus.Pending);
        entity.Id.Should().NotBeEmpty();
        entity.Protocol.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void U8_CalculationAndEligibility_WithoutPersistence_Should_BeTestable()
    {
        IAnticipationCalculationService calculation = new AnticipationCalculationService();
        IEligibilityService eligibility = new EligibilityService();
        var receivables = new List<ReceivableInfo>
        {
            new(Guid.NewGuid(), 1000m, DateTime.UtcNow.AddDays(5), ReceivableStatus.Eligible)
        };
        var eligible = eligibility.FilterEligible(receivables);
        var result = calculation.Calculate(500m, eligible);

        result.Should().NotBeNull();
        result!.NetAmount.Should().BeLessThan(result.GrossAmount);
    }
}