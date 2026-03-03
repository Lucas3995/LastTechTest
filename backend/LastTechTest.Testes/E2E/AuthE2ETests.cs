using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LastTechTest.API;
using LastTechTest.Aplicacao.Authentication.Commands.Login;
using LastTechTest.Aplicacao.Authentication.Commands.RegisterUser;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LastTechTest.Testes.E2E;

public class AuthE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { });
    }

    [Fact]
    public async Task Full_Auth_Flow_Should_Succeed()
    {
        var client = _factory.CreateClient();

        var email = $"e2e-{Guid.NewGuid():N}@example.com";
        var password = "StrongPassword123!";

        var registerResponse = await client.PostAsJsonAsync("/auth/register", new RegisterUserCommand(email, password));
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new LoginCommand(email, password));
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

    private sealed class AuthTokensDtoLike
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

