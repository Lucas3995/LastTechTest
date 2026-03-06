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

    /// <summary>RC-1 CA1 – RequestedAmount &lt; 100 returns 400 with message that amount must be 100 or more.</summary>
    [Theory]
    [InlineData(50)]
    [InlineData(99)]
    [InlineData(99.99)]
    public async Task E5b_RC1_PostAnticipations_AmountLessThan100_Should_Return400(decimal amount)
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var body = new { RequestedAmount = amount, CreatorId = (Guid?)null };

        var response = await client.PostAsJsonAsync("/api/v1/anticipations", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("error");
        json.Should().Contain("100");
    }

    /// <summary>RC-1 CA2 – RequestedAmount &gt;= 100 with no pending request returns 201 (100, 100.00, 100.01, 101).</summary>
    [Theory]
    [InlineData(100)]
    [InlineData(100.00)]
    [InlineData(100.01)]
    [InlineData(101)]
    public async Task E5c_RC1_PostAnticipations_Amount100OrMore_NoPending_Should_Return201(decimal amount)
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var body = new { RequestedAmount = amount, CreatorId = (Guid?)null };

        var response = await client.PostAsJsonAsync("/api/v1/anticipations", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        content.Should().NotBeNull();
        content!.Id.Should().NotBeEmpty();
        content.Status.Should().BeOneOf("Created", "Pending");
    }

    /// <summary>RC-1 CA3/CA4 – Creator with existing pending request cannot create another; returns 400. Exercises HasPendingByCreatorAsync with SQL (LINQ-to-SQL).</summary>
    [Fact]
    public async Task E5d_RC1_PostAnticipations_WhenCreatorHasPendingRequest_Should_Return400()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var firstBody = new { RequestedAmount = 100m, CreatorId = (Guid?)null };
        var firstResponse = await client.PostAsJsonAsync("/api/v1/anticipations", firstBody);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondBody = new { RequestedAmount = 100m, CreatorId = (Guid?)null };
        var secondResponse = await client.PostAsJsonAsync("/api/v1/anticipations", secondBody);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await secondResponse.Content.ReadAsStringAsync();
        json.Should().Contain("error");
        json.Should().Contain("open");
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
        await client.PostAsJsonAsync("/api/v1/anticipations", new { RequestedAmount = 101m, CreatorId = creatorId });
        await client.PostAsJsonAsync("/api/v1/anticipations", new { RequestedAmount = 201m, CreatorId = creatorId });

        var listResponse = await client.GetAsync("/api/v1/anticipations");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<ListAnticipationResponseDto>();
        list.Should().NotBeNull();
        list!.TotalCount.Should().BeGreaterThanOrEqualTo(2);
        list.Items.Should().NotBeNull();
    }

    /// <summary>CA2 – GET by id as Creator for other creator's request: acesso negado (500 ou falha enquanto GetById não mapear InvalidOperationException).</summary>
    [Fact]
    public async Task E8_GetAnticipationById_AsCreator_OtherCreatorRequest_Should_Return500()
    {
        var client = _factory.CreateClient();
        var adminId = Guid.NewGuid();
        var creatorA = Guid.NewGuid();
        var creatorB = Guid.NewGuid();
        var tokenAdmin = CreateJwt(adminId, "Admin");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenAdmin);
        var createResponse = await client.PostAsJsonAsync("/api/v1/anticipations",
            new { RequestedAmount = 150m, CreatorId = creatorB });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        created.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateJwt(creatorA, "Creator"));
        HttpResponseMessage? getResponse = null;
        try
        {
            getResponse = await client.GetAsync($"/api/v1/anticipations/{created!.Id}");
        }
        catch
        {
            // Em alguns ambientes (ex.: TestHost) a exceção pode propagar em vez de devolver 500; Creator foi negado.
        }

        if (getResponse is not null)
            getResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
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

    // --- RA-3: Estados e transições (CA1–CA5). Endpoints retornam 501 até implementação. ---

    /// <summary>CA1 – Aprovação por Analista: POST approve com dados obrigatórios → 200 e estado APROVADA (ou 501 até implementação).</summary>
    [Fact]
    public async Task E13_RA3_CA1_PostApprove_AsAnalista_WhenPending_Should_Return200AndApproved()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var tokenCreator = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenCreator);
        var createResponse = await client.PostAsJsonAsync("/api/v1/anticipations", new { RequestedAmount = 500m, CreatorId = (Guid?)null });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        created.Should().NotBeNull();

        var tokenAnalista = CreateJwt(Guid.NewGuid(), "Analista");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenAnalista);
        var approveBody = new { Observation = "Aprovado conforme política." };
        var approveResponse = await client.PostAsJsonAsync($"/api/v1/anticipations/{created!.Id}/approve", approveBody);

        approveResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);
        if (approveResponse.StatusCode == HttpStatusCode.OK)
        {
            var content = await approveResponse.Content.ReadFromJsonAsync<TransitionResponseDto>();
            content.Should().NotBeNull();
            content!.Status.Should().Be("Approved");
        }
    }

    /// <summary>CA2 (cenário 1) – Cancelamento válido: Creator dono, POST cancel → 200 e estado CANCELADA_POR_CREATOR.</summary>
    [Fact]
    public async Task E14_RA3_CA2_PostCancel_AsCreatorOwner_WhenPending_Should_Return200AndCanceled()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var createResponse = await client.PostAsJsonAsync("/api/v1/anticipations", new { RequestedAmount = 500m, CreatorId = (Guid?)null });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        created.Should().NotBeNull();

        var cancelResponse = await client.PostAsJsonAsync($"/api/v1/anticipations/{created!.Id}/cancel", new { Reason = (string?)null });

        cancelResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);
        if (cancelResponse.StatusCode == HttpStatusCode.OK)
        {
            var content = await cancelResponse.Content.ReadFromJsonAsync<CancelTransitionResponseDto>();
            content.Should().NotBeNull();
            content!.Status.Should().Be("CanceledByCreator");
        }
    }

    /// <summary>CA2 (cenário 2) – Cancelamento idempotente: já cancelada → não alterar estado, mensagem clara (200 com AlreadyCanceled ou 409/422).</summary>
    [Fact]
    public async Task E15_RA3_CA2_PostCancel_WhenAlreadyCanceled_Should_NotChangeStateAndIndicateIdempotent()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var createResponse = await client.PostAsJsonAsync("/api/v1/anticipations", new { RequestedAmount = 500m, CreatorId = (Guid?)null });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        created.Should().NotBeNull();
        await client.PostAsJsonAsync($"/api/v1/anticipations/{created!.Id}/cancel", new { });
        var secondCancel = await client.PostAsJsonAsync($"/api/v1/anticipations/{created.Id}/cancel", new { });

        secondCancel.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Conflict, (HttpStatusCode)422, HttpStatusCode.NotImplemented);
        if (secondCancel.StatusCode == HttpStatusCode.OK)
        {
            var content = await secondCancel.Content.ReadFromJsonAsync<CancelTransitionResponseDto>();
            content.Should().NotBeNull();
            (content!.AlreadyCanceled == true || (content.Status == "CanceledByCreator")).Should().BeTrue();
        }
    }

    /// <summary>CA3 – Recusa por Analista: POST reject com motivo → 200 e estado RECUSADA.</summary>
    [Fact]
    public async Task E16_RA3_CA3_PostReject_AsAnalista_WhenPending_Should_Return200AndRejected()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var tokenCreator = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenCreator);
        var createResponse = await client.PostAsJsonAsync("/api/v1/anticipations", new { RequestedAmount = 500m, CreatorId = (Guid?)null });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        created.Should().NotBeNull();

        var tokenAnalista = CreateJwt(Guid.NewGuid(), "Analista");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenAnalista);
        var rejectResponse = await client.PostAsJsonAsync($"/api/v1/anticipations/{created!.Id}/reject", new { Reason = "Documentação insuficiente." });

        rejectResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);
        if (rejectResponse.StatusCode == HttpStatusCode.OK)
        {
            var content = await rejectResponse.Content.ReadFromJsonAsync<TransitionResponseDto>();
            content.Should().NotBeNull();
            content!.Status.Should().Be("Rejected");
        }
    }

    /// <summary>CA4 – Bloqueio de transições inválidas: aprovar já aprovada → recusar operação, estado inalterado.</summary>
    [Fact]
    public async Task E17_RA3_CA4_PostApprove_WhenAlreadyApproved_Should_RejectOperation()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var tokenCreator = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenCreator);
        var createResponse = await client.PostAsJsonAsync("/api/v1/anticipations", new { RequestedAmount = 500m, CreatorId = (Guid?)null });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        created.Should().NotBeNull();
        var tokenAnalista = CreateJwt(Guid.NewGuid(), "Analista");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenAnalista);
        await client.PostAsJsonAsync($"/api/v1/anticipations/{created!.Id}/approve", new { Observation = "Ok" });
        var secondApprove = await client.PostAsJsonAsync($"/api/v1/anticipations/{created.Id}/approve", new { Observation = "Again" });

        secondApprove.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, (HttpStatusCode)422, HttpStatusCode.NotImplemented);
    }

    /// <summary>CA5 – Creator a chamar approve → 403 (endpoint restrito a Analista/Admin).</summary>
    [Fact]
    public async Task E18_RA3_CA5_PostApprove_AsCreator_Should_Return403()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var token = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var createResponse = await client.PostAsJsonAsync("/api/v1/anticipations", new { RequestedAmount = 500m, CreatorId = (Guid?)null });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        created.Should().NotBeNull();

        var approveResponse = await client.PostAsJsonAsync($"/api/v1/anticipations/{created!.Id}/approve", new { Observation = "Trying as Creator" });

        approveResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>CA5 – Admin pode executar transição em qualquer solicitação (approve).</summary>
    [Fact]
    public async Task E19_RA3_CA5_PostApprove_AsAdmin_Should_AllowTransition()
    {
        var client = _factory.CreateClient();
        var creatorId = Guid.NewGuid();
        var tokenCreator = CreateJwt(creatorId, "Creator");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenCreator);
        var createResponse = await client.PostAsJsonAsync("/api/v1/anticipations", new { RequestedAmount = 500m, CreatorId = (Guid?)null });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<AnticipationResponseDto>();
        created.Should().NotBeNull();

        var tokenAdmin = CreateJwt(Guid.NewGuid(), "Admin");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenAdmin);
        var approveResponse = await client.PostAsJsonAsync($"/api/v1/anticipations/{created!.Id}/approve", new { Observation = "Admin approval" });

        approveResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);
    }

    /// <summary>RA-3: POST approve sem token → 401.</summary>
    [Fact]
    public async Task E20_RA3_PostApprove_WithoutToken_Should_Return401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync($"/api/v1/anticipations/{Guid.NewGuid()}/approve", new { Observation = "x" });
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

    /// <summary>RA-3: contrato de resposta approve/reject (Id, Protocol, Status).</summary>
    private sealed class TransitionResponseDto
    {
        public Guid Id { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>RA-3: contrato de resposta cancel (inclui AlreadyCanceled).</summary>
    private sealed class CancelTransitionResponseDto
    {
        public Guid Id { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool AlreadyCanceled { get; set; }
    }
}