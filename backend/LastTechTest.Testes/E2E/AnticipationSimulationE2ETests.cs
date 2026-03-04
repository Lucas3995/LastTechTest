using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

using FluentAssertions;

using LastTechTest.API;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace LastTechTest.Testes.E2E;

/// <summary>RA-4 CA1–CA8: E2E for simulation and convert-to-real endpoints. Accepts 501 until implementation.</summary>
[Trait("Category", "E2E")]
public class AnticipationSimulationE2ETests : IClassFixture<CustomWebApplicationFactory>
{
    private const string TestJwtSecret = "change-me-in-production-super-secret-key";
    private const string TestIssuer = "LastTechTest";
    private const string TestAudience = "LastTechTest-Users";
    private readonly CustomWebApplicationFactory _factory;

    public AnticipationSimulationE2ETests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>CA1: POST /api/v1/anticipations/simulations as Creator, valid body — 200 with simulationCode, validUntilUtc, financials (or 501 until implemented).</summary>
    [Fact]
    public async Task RA4_CA1_PostSimulations_AsCreator_ValidBody_Should_Return200WithSimulationCodeAndValidity()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var body = new { RequestedAmount = 1000m, CreatorId = (Guid?)null, RequestedAtUtc = (DateTime?)null };

        var response = await client.PostAsJsonAsync("/api/v1/anticipations/simulations", body);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadFromJsonAsync<SimulationResponseDto>();
            content.Should().NotBeNull();
            content!.SimulationCode.Should().NotBeNullOrWhiteSpace();
            content.ValidUntilUtc.Should().BeCloseTo(DateTime.UtcNow.AddHours(2).AddMinutes(-20), TimeSpan.FromMinutes(2));
            content.RequestedAmount.Should().Be(1000m);
            content.GrossAmount.Should().BeGreaterThan(0);
            content.FeesAmount.Should().BeGreaterThan(0);
            content.NetAmount.Should().BeGreaterThan(0);
        }
    }

    /// <summary>CA2: POST simulations with RequestedAmount &lt; 100 or other invalid — 400/422.</summary>
    [Theory]
    [InlineData(50)]
    [InlineData(99)]
    [InlineData(99.99)]
    public async Task RA4_CA2_PostSimulations_InvalidAmount_Should_Return400(decimal amount)
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var body = new { RequestedAmount = amount, CreatorId = (Guid?)null };

        var response = await client.PostAsJsonAsync("/api/v1/anticipations/simulations", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("error");
        json.Should().Contain("100");
    }

    /// <summary>CA3: Multiple simulation calls do not create anticipation records — GET list count unchanged (or zero for new creator).</summary>
    [Fact]
    public async Task RA4_CA3_PostSimulations_MultipleTimes_Should_NotCreateAnticipationRecords()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var body = new { RequestedAmount = 500m, CreatorId = (Guid?)null };

        await client.PostAsJsonAsync("/api/v1/anticipations/simulations", body);
        await client.PostAsJsonAsync("/api/v1/anticipations/simulations", body);

        var listResponse = await client.GetAsync("/api/v1/anticipations");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<ListAnticipationResponseDto>();
        list.Should().NotBeNull();
        list!.Items.Should().NotBeNull();
        list.Items!.Length.Should().Be(0);
        list.TotalCount.Should().Be(0);
    }

    /// <summary>CA4: POST simulations as Analista with creatorId (on behalf of) — 200 or 501.</summary>
    [Fact]
    public async Task RA4_CA4_PostSimulations_AsAnalista_WithCreatorId_Should_Return200()
    {
        var client = _factory.CreateClient();
        var analistaId = Guid.NewGuid();
        var targetCreatorId = Guid.NewGuid();
        var token = CreateJwt(analistaId, "Analista");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var body = new { RequestedAmount = 200m, CreatorId = targetCreatorId };

        var response = await client.PostAsJsonAsync("/api/v1/anticipations/simulations", body);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);
    }

    /// <summary>CA4: POST simulations as Admin with creatorId — 200 or 501.</summary>
    [Fact]
    public async Task RA4_CA4_PostSimulations_AsAdmin_WithCreatorId_Should_Return200()
    {
        var client = _factory.CreateClient();
        var adminId = Guid.NewGuid();
        var targetCreatorId = Guid.NewGuid();
        var token = CreateJwt(adminId, "Admin");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var body = new { RequestedAmount = 300m, CreatorId = targetCreatorId };

        var response = await client.PostAsJsonAsync("/api/v1/anticipations/simulations", body);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);
    }

    /// <summary>CA6 + CA7/CA8: Simulate twice (second replaces first); confirm with first code fails (404/410); confirm with second succeeds (201) when implemented. For now accept 501.</summary>
    [Fact]
    public async Task RA4_CA6_SimulateTwice_ConfirmWithFirstCode_Fails_ConfirmWithSecond_SucceedsOr501()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var sim1Body = new { RequestedAmount = 100m, CreatorId = (Guid?)null };
        var sim1 = await client.PostAsJsonAsync("/api/v1/anticipations/simulations", sim1Body);
        var sim2Body = new { RequestedAmount = 200m, CreatorId = (Guid?)null };
        var sim2 = await client.PostAsJsonAsync("/api/v1/anticipations/simulations", sim2Body);

        string? codeA = null;
        string? codeB = null;
        if (sim1.StatusCode == HttpStatusCode.OK)
        {
            var c1 = await sim1.Content.ReadFromJsonAsync<SimulationResponseDto>();
            codeA = c1?.SimulationCode;
        }
        if (sim2.StatusCode == HttpStatusCode.OK)
        {
            var c2 = await sim2.Content.ReadFromJsonAsync<SimulationResponseDto>();
            codeB = c2?.SimulationCode;
        }

        if (codeA != null)
        {
            var confirmFirst = await client.PostAsync($"/api/v1/anticipations/simulations/{codeA}/confirm", null);
            confirmFirst.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, (HttpStatusCode)410, HttpStatusCode.NotImplemented);
        }

        if (codeB != null)
        {
            var confirmSecond = await client.PostAsync($"/api/v1/anticipations/simulations/{codeB}/confirm", null);
            confirmSecond.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.NotImplemented);
        }
    }

    /// <summary>CA7: POST sim → POST confirm (no other open request) → 201 and created request with same values. (Or 501 until implemented.)</summary>
    [Fact]
    public async Task RA4_CA7_PostSimulation_ThenConfirm_Should_Return201AndCreatedRequestWithIdenticalValues()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var simBody = new { RequestedAmount = 500m, CreatorId = (Guid?)null };
        var simResponse = await client.PostAsJsonAsync("/api/v1/anticipations/simulations", simBody);

        if (simResponse.StatusCode != HttpStatusCode.OK)
        {
            simResponse.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
            return;
        }

        var simContent = await simResponse.Content.ReadFromJsonAsync<SimulationResponseDto>();
        simContent.Should().NotBeNull();
        var confirmResponse = await client.PostAsync($"/api/v1/anticipations/simulations/{simContent!.SimulationCode}/confirm", null);
        confirmResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.NotImplemented);
        if (confirmResponse.StatusCode == HttpStatusCode.Created)
        {
            var created = await confirmResponse.Content.ReadFromJsonAsync<AnticipationResponseDto>();
            created.Should().NotBeNull();
            created!.NetAmount.Should().Be(simContent.NetAmount);
            var getResponse = await client.GetAsync($"/api/v1/anticipations/{created.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var detail = await getResponse.Content.ReadFromJsonAsync<AnticipationDetailDto>();
            detail.Should().NotBeNull();
            detail!.RequestedAmount.Should().Be(simContent.RequestedAmount);
            detail.GrossAmount.Should().Be(simContent.GrossAmount);
            detail.FeesAmount.Should().Be(simContent.FeesAmount);
            detail.NetAmount.Should().Be(simContent.NetAmount);
        }
    }

    /// <summary>CA8: Confirm with nonexistent code → 404/410/501.</summary>
    [Fact]
    public async Task RA4_CA8_Confirm_NonexistentCode_Should_Return404Or410()
    {
        var client = _factory.CreateClient();
        var token = CreateJwt(Guid.NewGuid(), "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsync("/api/v1/anticipations/simulations/NONEXISTENT/confirm", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, (HttpStatusCode)410, HttpStatusCode.NotImplemented);
    }

    /// <summary>CA8: Confirm with already-used code → 422 or 501.</summary>
    [Fact]
    public async Task RA4_CA8_Confirm_AlreadyUsedCode_Should_Return422()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var simBody = new { RequestedAmount = 100m, CreatorId = (Guid?)null };
        var simResponse = await client.PostAsJsonAsync("/api/v1/anticipations/simulations", simBody);
        if (simResponse.StatusCode != HttpStatusCode.OK)
        {
            return;
        }
        var simContent = await simResponse.Content.ReadFromJsonAsync<SimulationResponseDto>();
        var code = simContent?.SimulationCode;
        if (code == null) return;
        await client.PostAsync($"/api/v1/anticipations/simulations/{code}/confirm", null);
        var secondConfirm = await client.PostAsync($"/api/v1/anticipations/simulations/{code}/confirm", null);
        secondConfirm.StatusCode.Should().BeOneOf((HttpStatusCode)422, HttpStatusCode.NotImplemented);
    }

    /// <summary>CA8: Creator with open request then confirm simulation → 422 (already has open request).</summary>
    [Fact]
    public async Task RA4_CA8_Confirm_WhenCreatorHasOpenRequest_Should_Return422()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var createReal = await client.PostAsJsonAsync("/api/v1/anticipations", new { RequestedAmount = 100m, CreatorId = (Guid?)null });
        createReal.StatusCode.Should().Be(HttpStatusCode.Created);
        var simResponse = await client.PostAsJsonAsync("/api/v1/anticipations/simulations", new { RequestedAmount = 200m, CreatorId = (Guid?)null });
        if (simResponse.StatusCode != HttpStatusCode.OK) return;
        var simContent = await simResponse.Content.ReadFromJsonAsync<SimulationResponseDto>();
        if (simContent?.SimulationCode == null) return;
        var confirmResponse = await client.PostAsync($"/api/v1/anticipations/simulations/{simContent.SimulationCode}/confirm", null);
        confirmResponse.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, (HttpStatusCode)422, HttpStatusCode.NotImplemented);
    }

    /// <summary>POST simulations without token → 401.</summary>
    [Fact]
    public async Task RA4_PostSimulations_WithoutToken_Should_Return401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/anticipations/simulations", new { RequestedAmount = 1000m, CreatorId = (Guid?)null });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static string CreateJwt(Guid userId, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.Role, role),
            new("role", role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            TestIssuer,
            TestAudience,
            claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class SimulationResponseDto
    {
        public string SimulationCode { get; set; } = string.Empty;
        public DateTime ValidUntilUtc { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal FeesAmount { get; set; }
        public decimal NetAmount { get; set; }
    }

    private sealed class AnticipationResponseDto
    {
        public Guid Id { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public decimal NetAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private sealed class AnticipationDetailDto
    {
        public Guid Id { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public Guid CreatorId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal FeesAmount { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime RequestedAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    private sealed class ListAnticipationItemDto
    {
        public Guid Id { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public Guid CreatorId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public decimal NetAmount { get; set; }
        public DateTime RequestedAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    private sealed class ListAnticipationResponseDto
    {
        public ListAnticipationItemDto[]? Items { get; set; }
        public int TotalCount { get; set; }
    }
}
