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
}
