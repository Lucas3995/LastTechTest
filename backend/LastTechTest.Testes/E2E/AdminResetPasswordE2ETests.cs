using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using LastTechTest.API;

using Microsoft.AspNetCore.Mvc.Testing;

namespace LastTechTest.Testes.E2E;

[Trait("Category", "E2E")]
public class AdminResetPasswordE2ETests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AdminResetPasswordE2ETests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_ResetPassword_ByEmail_User_Can_Login_With_DefaultPassword()
    {
        var client = _factory.CreateClient();

        var adminLogin = await client.PostAsJsonAsync("/auth/login", new
        {
            Email = "usu_acesso_total@example.com",
            Password = "Acess0@t0ta1"
        });
        adminLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminTokens = await adminLogin.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens!.AccessToken);

        var email = $"reset-by-email-{Guid.NewGuid():N}@example.com";
        var createResponse = await client.PostAsJsonAsync("/auth/admin/users", new { Email = email, Roles = new[] { "Creator" } });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        client.DefaultRequestHeaders.Authorization = null;
        var loginWithDefault = await client.PostAsJsonAsync("/auth/login", new { Email = email, Password = "Trocar@123" });
        loginWithDefault.StatusCode.Should().Be(HttpStatusCode.OK);
        var userTokens = await loginWithDefault.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userTokens!.AccessToken);

        var newPassword = "NewStrongPass1!";
        var changeResponse = await client.PostAsJsonAsync("/auth/change-password", new
        {
            CurrentPassword = "Trocar@123",
            NewPassword = newPassword
        });
        changeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens.AccessToken);
        var resetResponse = await client.PostAsJsonAsync("/auth/admin/users/reset-password", new { Email = email });
        resetResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        client.DefaultRequestHeaders.Authorization = null;
        var loginOldPassword = await client.PostAsJsonAsync("/auth/login", new { Email = email, Password = newPassword });
        loginOldPassword.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var loginDefaultPassword = await client.PostAsJsonAsync("/auth/login", new { Email = email, Password = "Trocar@123" });
        loginDefaultPassword.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Admin_ResetPassword_ById_User_Can_Login_With_DefaultPassword()
    {
        var client = _factory.CreateClient();

        var adminLogin = await client.PostAsJsonAsync("/auth/login", new
        {
            Email = "usu_acesso_total@example.com",
            Password = "Acess0@t0ta1"
        });
        adminLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminTokens = await adminLogin.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens!.AccessToken);

        var email = $"reset-by-id-{Guid.NewGuid():N}@example.com";
        var createResponse = await client.PostAsJsonAsync("/auth/admin/users", new { Email = email, Roles = new[] { "Creator" } });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createBody = await createResponse.Content.ReadFromJsonAsync<CreateUserResponseDtoLike>();
        var userId = createBody!.Id;

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens.AccessToken);
        var resetResponse = await client.PostAsJsonAsync("/auth/admin/users/reset-password", new { Id = userId });
        resetResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new { Email = email, Password = "Trocar@123" });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task NonAdmin_Should_Not_Reset_Password()
    {
        var client = _factory.CreateClient();

        var adminLogin = await client.PostAsJsonAsync("/auth/login", new
        {
            Email = "usu_acesso_total@example.com",
            Password = "Acess0@t0ta1"
        });
        adminLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminTokens = await adminLogin.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens!.AccessToken);

        var nonAdminEmail = $"non-admin-reset-{Guid.NewGuid():N}@example.com";
        var createResponse = await client.PostAsJsonAsync("/auth/admin/users", new { Email = nonAdminEmail, Roles = new[] { "Creator" } });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new { Email = nonAdminEmail, Password = "Trocar@123" });
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        var resetResponse = await client.PostAsJsonAsync("/auth/admin/users/reset-password", new { Email = nonAdminEmail });

        resetResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ResetPassword_WhenUserNotFound_Returns_400()
    {
        var client = _factory.CreateClient();

        var adminLogin = await client.PostAsJsonAsync("/auth/login", new
        {
            Email = "usu_acesso_total@example.com",
            Password = "Acess0@t0ta1"
        });
        adminLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminTokens = await adminLogin.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens!.AccessToken);

        var resetResponse = await client.PostAsJsonAsync("/auth/admin/users/reset-password", new { Email = "nonexistent@example.com" });

        resetResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed class AuthTokensDtoLike
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    private sealed class CreateUserResponseDtoLike
    {
        public Guid Id { get; set; }
    }
}