using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OpsFlow.Application.DTOs.Auth;
using OpsFlow.Domain.Entities;
using OpsFlow.Infrastructure.Persistence;

namespace OpsFlow.Tests.Integration.Authentication;

public class AuthEndpointsTests : IClassFixture<global::OpsFlow.Tests.RequestAuditEndpointTests.TestFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly global::OpsFlow.Tests.RequestAuditEndpointTests.TestFactory _factory;

    public AuthEndpointsTests(global::OpsFlow.Tests.RequestAuditEndpointTests.TestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ReturnsOkAndToken()
    {
        await ResetDatabaseAsync();

        var payload = new
        {
            name = "Auth Employee",
            email = "auth.employee@test.local",
            password = "P@ssw0rd!",
            role = "Employee"
        };

        using var response = await PostJsonAsync("/api/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await DeserializeAsync<AuthResponse>(response);
        auth.Should().NotBeNull();
        auth!.Email.Should().Be("auth.employee@test.local");
        auth.Role.Should().Be("Employee");
        auth.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_ReturnsJwtForRegisteredUser()
    {
        await ResetDatabaseAsync();

        var registerPayload = new
        {
            name = "Login User",
            email = "auth.login@test.local",
            password = "P@ssw0rd!",
            role = "Employee"
        };

        using var registerResponse = await PostJsonAsync("/api/auth/register", registerPayload);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginPayload = new
        {
            email = "auth.login@test.local",
            password = "P@ssw0rd!"
        };

        using var loginResponse = await PostJsonAsync("/api/auth/login", loginPayload);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await DeserializeAsync<AuthResponse>(loginResponse);
        auth.Should().NotBeNull();
        auth!.Token.Should().NotBeNullOrWhiteSpace();
        auth.Email.Should().Be("auth.login@test.local");
    }

    [Fact]
    public async Task Me_ReturnsCurrentUser_WhenAuthenticated()
    {
        await ResetDatabaseAsync();

        var registerPayload = new
        {
            name = "Current User",
            email = "auth.me@test.local",
            password = "P@ssw0rd!",
            role = "Employee"
        };

        using var registerResponse = await PostJsonAsync("/api/auth/register", registerPayload);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await DeserializeAsync<AuthResponse>(registerResponse);
        auth.Should().NotBeNull();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        using var meResponse = await client.GetAsync("/api/auth/me");

        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var current = await DeserializeAsync<CurrentUserResponse>(meResponse);
        current.Should().NotBeNull();
        current!.Email.Should().Be("auth.me@test.local");
        current.Name.Should().Be("Current User");
        current.Role.Should().Be("Employee");
    }

    [Fact]
    public async Task Me_ReturnsUnauthorized_WhenAnonymous()
    {
        await ResetDatabaseAsync();

        var client = _factory.CreateClient();
        using var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Employee_CallingManagerEndpoint_ReturnsForbidden()
    {
        await ResetDatabaseAsync();

        var registerPayload = new
        {
            name = "Employee User",
            email = "auth.forbidden@test.local",
            password = "P@ssw0rd!",
            role = "Employee"
        };

        using var registerResponse = await PostJsonAsync("/api/auth/register", registerPayload);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await DeserializeAsync<AuthResponse>(registerResponse);
        auth.Should().NotBeNull();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        using var response = await client.GetAsync("/api/manager/approvals");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task ResetDatabaseAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    private async Task<HttpResponseMessage> PostJsonAsync(string route, object payload)
    {
        var client = _factory.CreateClient();
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(route, content);
    }

    private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }
}
