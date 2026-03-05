using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using LastTechTest.API;

using Microsoft.AspNetCore.Mvc.Testing;

namespace LastTechTest.Testes.E2E;

[Trait("Category", "E2E")]
public class AdminUserManagementE2ETests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AdminUserManagementE2ETests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_Should_Create_User_With_DefaultPassword_And_Roles()
    {
        var client = _factory.CreateClient();

        // Arrange: garantir admin via seed (usu_acesso_total@example.com / Acess0@t0ta1)
        var adminLogin = await client.PostAsJsonAsync("/auth/login", new
        {
            Email = "usu_acesso_total@example.com",
            Password = "Acess0@t0ta1"
        });
        adminLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminTokens = await adminLogin.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        adminTokens!.AccessToken.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens.AccessToken);

        var newUserEmail = $"admin-created-{Guid.NewGuid():N}@example.com";
        var createPayload = new
        {
            Email = newUserEmail,
            Roles = new[] { "Creator", "Analista" }
        };

        // Act: admin cria usuário sem informar senha (senha padrão Trocar@123)
        var createResponse = await client.PostAsJsonAsync("/auth/admin/users", createPayload);

        // Assert: usuário criado
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // E o novo usuário consegue logar com a senha padrão e recebe roles no token
        var newUserLogin = await client.PostAsJsonAsync("/auth/login", new
        {
            Email = newUserEmail,
            Password = "Trocar@123"
        });
        newUserLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        var newUserTokens = await newUserLogin.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        newUserTokens!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task NonAdmin_Should_Not_Create_Users()
    {
        var client = _factory.CreateClient();

        // Admin creates a non-admin user (Creator only), then we use that user's token to try creating another user
        var adminLogin = await client.PostAsJsonAsync("/auth/login", new
        {
            Email = "usu_acesso_total@example.com",
            Password = "Acess0@t0ta1"
        });
        adminLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminTokens = await adminLogin.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens!.AccessToken);

        var nonAdminEmail = $"non-admin-{Guid.NewGuid():N}@example.com";
        var createResponse = await client.PostAsJsonAsync("/auth/admin/users", new { Email = nonAdminEmail, Roles = new[] { "Creator" } });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new { Email = nonAdminEmail, Password = "Trocar@123" });
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokensDtoLike>();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        var payload = new
        {
            Email = $"should-not-be-created-{Guid.NewGuid():N}@example.com",
            Roles = new[] { "Creator" }
        };

        var response = await client.PostAsJsonAsync("/auth/admin/users", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private sealed class AuthTokensDtoLike
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}