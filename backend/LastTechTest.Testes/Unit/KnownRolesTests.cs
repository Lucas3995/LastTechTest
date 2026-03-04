using FluentAssertions;

using LastTechTest.Dominio.Authorization;

namespace LastTechTest.Testes.Unit;

[Trait("Category", "Unit")]
public class KnownRolesTests
{
    [Fact]
    public void KnownRoles_Should_Expose_Expected_Names()
    {
        KnownRoles.Admin.Should().Be("Admin");
        KnownRoles.Creator.Should().Be("Creator");
        KnownRoles.Analista.Should().Be("Analista");
    }
}

