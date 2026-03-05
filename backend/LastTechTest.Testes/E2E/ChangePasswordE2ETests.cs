using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using LastTechTest.API;

using Microsoft.AspNetCore.Mvc.Testing;

namespace LastTechTest.Testes.E2E;

[Trait("Category", "E2E")]
public class ChangePasswordE2ETests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ChangePasswordE2ETests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ChangePassword_Should_Allow_User_To_Login_With_NewPassword_Only()
    {
        var client = _factory.CreateClient();

        var adminLogin = await client.PostAsJsonAsync("/auth/login", new { Email = "usu_acesso_total@example.com", Password = "Acess0@t0ta1" });
        adminLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminTokens = await adminLogin.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens!.AccessToken);

        var email = $"change-pass-{Guid.NewGuid():N}@example.com";
        var createResponse = await client.PostAsJsonAsync("/auth/admin/users", new { Email = email, Roles = new[] { "Creator" } });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        const string originalPassword = "Trocar@123";
        client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new { Email = email, Password = originalPassword });
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokensDtoLike>();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        var newPassword = "NewStrongPass1!";
        var changePayload = new
        {
            CurrentPassword = originalPassword,
            NewPassword = newPassword
        };

        var changeResponse = await client.PostAsJsonAsync("/auth/change-password", changePayload);
        changeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Não deve mais logar com a senha antiga
        var oldLogin = await client.PostAsJsonAsync("/auth/login", new { Email = email, Password = originalPassword });
        oldLogin.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Deve conseguir logar com a nova senha
        var newLogin = await client.PostAsJsonAsync("/auth/login", new { Email = email, Password = newPassword });
        newLogin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_With_WrongCurrentPassword_Should_Return_403_Or_400()
    {
        var client = _factory.CreateClient();

        var adminLogin = await client.PostAsJsonAsync("/auth/login", new { Email = "usu_acesso_total@example.com", Password = "Acess0@t0ta1" });
        adminLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminTokens = await adminLogin.Content.ReadFromJsonAsync<AuthTokensDtoLike>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminTokens!.AccessToken);

        var email = $"change-pass-wrong-{Guid.NewGuid():N}@example.com";
        var createResponse = await client.PostAsJsonAsync("/auth/admin/users", new { Email = email, Roles = new[] { "Creator" } });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new { Email = email, Password = "Trocar@123" });
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokensDtoLike>();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        var changePayload = new
        {
            CurrentPassword = "WrongPassword1!",
            NewPassword = "NewStrongPass1!"
        };

        var changeResponse = await client.PostAsJsonAsync("/auth/change-password", changePayload);
        changeResponse.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
    }

    private sealed class AuthTokensDtoLike
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}