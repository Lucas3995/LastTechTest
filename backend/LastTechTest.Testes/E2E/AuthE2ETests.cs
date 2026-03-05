using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using LastTechTest.API;
using LastTechTest.Aplicacao.Authentication.Commands.Login;
using LastTechTest.Aplicacao.Authentication.Commands.Logout;
using LastTechTest.Aplicacao.Authentication.Commands.RefreshToken;

using Microsoft.AspNetCore.Mvc.Testing;

namespace LastTechTest.Testes.E2E;

[Trait("Category", "E2E")]
public class AuthE2ETests : IClassFixture<CustomWebApplicationFactory>
{
    private const string AdminEmail = "usu_acesso_total@example.com";
    private const string AdminPassword = "Acess0@t0ta1";
    private const string DefaultNewUserPassword = "Trocar@123";

    private readonly CustomWebApplicationFactory _factory;

    public AuthE2ETests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Full_Auth_Flow_Should_Succeed()
    {
        var client = _factory.CreateClient();

        var adminLogin = await client.PostAsJsonAsync("/auth/login", new { Email = AdminEmail, Password = AdminPassword });
        adminLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminTokens = await adminLogin.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        adminTokens!.AccessToken.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens.AccessToken);

        var newUserEmail = $"e2e-{Guid.NewGuid():N}@example.com";
        var createResponse = await client.PostAsJsonAsync("/auth/admin/users", new { Email = newUserEmail, Roles = new[] { "Creator" } });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new LoginCommand(newUserEmail, DefaultNewUserPassword));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        loginContent.Should().NotBeNull();
        loginContent!.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginContent.RefreshToken.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginContent.AccessToken);

        var meResponse = await client.GetAsync("/user/logged");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_Flow_Should_Return_New_Tokens()
    {
        var client = _factory.CreateClient();

        var adminLogin = await client.PostAsJsonAsync("/auth/login", new { Email = AdminEmail, Password = AdminPassword });
        adminLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminTokens = await adminLogin.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens!.AccessToken);

        var newUserEmail = $"e2e-refresh-{Guid.NewGuid():N}@example.com";
        var createResponse = await client.PostAsJsonAsync("/auth/admin/users", new { Email = newUserEmail, Roles = new[] { "Creator" } });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new LoginCommand(newUserEmail, DefaultNewUserPassword));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        loginContent.Should().NotBeNull();

        var refreshResponse = await client.PostAsJsonAsync("/auth/refresh", new RefreshTokenCommand(loginContent!.RefreshToken));
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshContent = await refreshResponse.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        refreshContent.Should().NotBeNull();
        refreshContent!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshContent.RefreshToken.Should().NotBeNullOrWhiteSpace();
        refreshContent.AccessToken.Should().NotBe(loginContent.AccessToken);
    }

    [Fact]
    public async Task Logout_Should_Accept_RefreshToken()
    {
        var client = _factory.CreateClient();

        var adminLogin = await client.PostAsJsonAsync("/auth/login", new { Email = AdminEmail, Password = AdminPassword });
        adminLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminTokens = await adminLogin.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens!.AccessToken);

        var newUserEmail = $"e2e-logout-{Guid.NewGuid():N}@example.com";
        var createResponse = await client.PostAsJsonAsync("/auth/admin/users", new { Email = newUserEmail, Roles = new[] { "Creator" } });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new LoginCommand(newUserEmail, DefaultNewUserPassword));
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<AuthTokensDtoLike>();

        var logoutRequest = new HttpRequestMessage(HttpMethod.Delete, "/auth/logout")
        {
            Content = JsonContent.Create(new LogoutCommand(loginContent!.RefreshToken))
        };
        var logoutResponse = await client.SendAsync(logoutRequest);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task User_Logged_Without_Token_Should_Return_401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/user/logged");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_CreateUser_Duplicate_Email_Should_Return_400()
    {
        var client = _factory.CreateClient();

        var adminLogin = await client.PostAsJsonAsync("/auth/login", new { Email = AdminEmail, Password = AdminPassword });
        adminLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminTokens = await adminLogin.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens!.AccessToken);

        var email = $"e2e-dup-{Guid.NewGuid():N}@example.com";
        var firstCreate = await client.PostAsJsonAsync("/auth/admin/users", new { Email = email, Roles = new[] { "Creator" } });
        firstCreate.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondCreate = await client.PostAsJsonAsync("/auth/admin/users", new { Email = email, Roles = new[] { "Creator" } });
        secondCreate.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Invalid_Refresh_Token_Should_Return_400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/refresh", new RefreshTokenCommand("invalid-refresh-token"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed class AuthTokensDtoLike
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}