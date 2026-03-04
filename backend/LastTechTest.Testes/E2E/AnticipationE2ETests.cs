using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

using FluentAssertions;

using LastTechTest.API;
using LastTechTest.Aplicacao.Authentication.Commands.Login;
using LastTechTest.Aplicacao.Authentication.Commands.RegisterUser;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace LastTechTest.Testes.E2E;

[Trait("Category", "E2E")]
public class AnticipationE2ETests : IClassFixture<CustomWebApplicationFactory>
{
    private const string TestJwtSecret = "change-me-in-production-super-secret-key";
    private const string TestIssuer = "LastTechTest";
    private const string TestAudience = "LastTechTest-Users";
    private readonly CustomWebApplicationFactory _factory;

    public AnticipationE2ETests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task E1_PostAnticipations_AsCreator_WithValidBody_Should_Return201AndContract()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var body = new { RequestedAmount = 1000m, CreatorId = (Guid?)null };

        var response = await client.PostAsJsonAsync("/api/v1/anticipations", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        content.Should().NotBeNull();
        content!.Id.Should().NotBeEmpty();
        content.Protocol.Should().NotBeNullOrWhiteSpace();
        content.NetAmount.Should().BeGreaterThan(0);
        content.NetAmount.Should().BeLessThanOrEqualTo(1000m);
        content.Status.Should().NotBeNullOrWhiteSpace();
        content.Status.Should().BeOneOf("Created", "Pending");
    }

    [Fact]
    public async Task E2_PostAnticipations_AsAdmin_WithCreatorId_Should_Return201AndContract()
    {
        var client = _factory.CreateClient();
        var adminUserId = Guid.NewGuid();
        var targetCreatorId = Guid.NewGuid();
        var token = CreateJwt(adminUserId, "Admin");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var body = new { RequestedAmount = 1000m, CreatorId = targetCreatorId };

        var response = await client.PostAsJsonAsync("/api/v1/anticipations", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        content.Should().NotBeNull();
        content!.Id.Should().NotBeEmpty();
        content.Protocol.Should().NotBeNullOrWhiteSpace();
        content.NetAmount.Should().BeGreaterThan(0);
        content.Status.Should().BeOneOf("Created", "Pending");
    }

    [Fact]
    public async Task E2b_PostAnticipations_RequestContract_OnlyThreeFields_ResponseContract_IncludesNetAmountAndStatus()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var requestBody = new { RequestedAmount = 500m, CreatorId = (Guid?)null, RequestedAtUtc = (DateTime?)null };

        var response = await client.PostAsJsonAsync("/api/v1/anticipations", requestBody);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        content.Should().NotBeNull();
        content!.Id.Should().NotBeEmpty();
        content.Protocol.Should().NotBeNullOrWhiteSpace();
        content.NetAmount.Should().BeGreaterThan(0);
        content.NetAmount.Should().BeLessThanOrEqualTo(500m);
        content.Status.Should().BeOneOf("Created", "Pending");
    }

    [Fact]
    public async Task E3_PostAnticipations_WithoutToken_Should_Return401()
    {
        var client = _factory.CreateClient();
        var body = new { RequestedAmount = 1000m };

        var response = await client.PostAsJsonAsync("/api/v1/anticipations", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task E4_PostAnticipations_AsCreator_WithOtherCreatorId_Should_Return403Or400()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var otherCreatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var body = new { RequestedAmount = 1000m, CreatorId = otherCreatorId };

        var response = await client.PostAsJsonAsync("/api/v1/anticipations", body);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task E5_PostAnticipations_InvalidEligibilityOrLimit_Should_Return400WithMessage()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var body = new { RequestedAmount = 100_000m };

        var response = await client.PostAsJsonAsync("/api/v1/anticipations", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("error");
    }

    // --- RA-2: GET list and GET by id ---

    /// <summary>CA1 – GET list as Creator returns only own requests.</summary>
    [Fact]
    public async Task E6_GetAnticipationsList_AsCreator_Should_ReturnOnlyOwn()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var postBody = new { RequestedAmount = 500m, CreatorId = (Guid?)null };
        var createResponse = await client.PostAsJsonAsync("/api/v1/anticipations", postBody);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await client.GetAsync("/api/v1/anticipations");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<ListAnticipationResponseDto>();
        list.Should().NotBeNull();
        list!.Items.Should().NotBeNull();
        list.Items.Should().HaveCount(1);
        list.Items![0].CreatorId.Should().Be(creatorId);
        list.TotalCount.Should().Be(1);
    }

    /// <summary>CA3 – GET list as Admin returns all (with or without filters).</summary>
    [Fact]
    public async Task E7_GetAnticipationsList_AsAdmin_Should_ReturnAll()
    {
        var client = _factory.CreateClient();
        var adminId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var tokenAdmin = CreateJwt(adminId, "Admin");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenAdmin);
        await client.PostAsJsonAsync("/api/v1/anticipations", new { RequestedAmount = 100m, CreatorId = creatorId });
        await client.PostAsJsonAsync("/api/v1/anticipations", new { RequestedAmount = 200m, CreatorId = creatorId });

        var listResponse = await client.GetAsync("/api/v1/anticipations");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<ListAnticipationResponseDto>();
        list.Should().NotBeNull();
        list!.TotalCount.Should().BeGreaterThanOrEqualTo(2);
        list.Items.Should().NotBeNull();
    }

    /// <summary>CA2 – GET by id as Creator for other creator's request returns 403.</summary>
    [Fact]
    public async Task E8_GetAnticipationById_AsCreator_OtherCreatorRequest_Should_Return403()
    {
        var client = _factory.CreateClient();
        var adminId = Guid.NewGuid();
        var creatorA = Guid.NewGuid();
        var creatorB = Guid.NewGuid();
        var tokenAdmin = CreateJwt(adminId, "Admin");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenAdmin);
        var createResponse = await client.PostAsJsonAsync("/api/v1/anticipations",
            new { RequestedAmount = 100m, CreatorId = creatorB });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        created.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateJwt(creatorA, "Creator"));
        var getResponse = await client.GetAsync($"/api/v1/anticipations/{created!.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>CA4 – GET by id as Admin returns 200 for any request.</summary>
    [Fact]
    public async Task E9_GetAnticipationById_AsAdmin_Should_Return200()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var createResponse = await client.PostAsJsonAsync("/api/v1/anticipations",
            new { RequestedAmount = 300m, CreatorId = (Guid?)null });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        created.Should().NotBeNull();

        var adminToken = CreateJwt(Guid.NewGuid(), "Admin");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var getResponse = await client.GetAsync($"/api/v1/anticipations/{created!.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await getResponse.Content.ReadFromJsonAsync<AnticipationDetailDto>();
        detail.Should().NotBeNull();
        detail!.Id.Should().Be(created.Id);
        detail.CreatorId.Should().Be(creatorId);
    }

    /// <summary>GET list without token returns 401.</summary>
    [Fact]
    public async Task E10_GetAnticipationsList_WithoutToken_Should_Return401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/anticipations");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>CA5 – GET list with pagination returns Items and TotalCount.</summary>
    [Fact]
    public async Task E11_GetAnticipationsList_WithPagination_Should_ReturnPagedStructure()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/anticipations?page=1&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<ListAnticipationResponseDto>();
        list.Should().NotBeNull();
        list!.Items.Should().NotBeNull();
        list.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    /// <summary>GET by id for nonexistent id returns 404.</summary>
    [Fact]
    public async Task E12_GetAnticipationById_NotFound_Should_Return404()
    {
        var client = _factory.CreateClient();
        var token = CreateJwt(Guid.NewGuid(), "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/anticipations/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

    /// <summary>Contrato de retorno do POST /api/v1/anticipations: Id, Protocol, NetAmount (valor_liquido), Status.</summary>
    private sealed class AnticipationResponseDto
    {
        public Guid Id { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public decimal NetAmount { get; set; }
        public string Status { get; set; } = string.Empty;
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
}