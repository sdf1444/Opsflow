using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using OpsFlow.Api;
using Xunit;

namespace OpsFlow.Tests;

public class AuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task EmployeeCannotAccessManagerEndpoints()
    {
        var client = CreateAuthenticatedClient("Employee");
        var response = await client.GetAsync("/api/manager/approvals");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EmployeeCannotAccessAdminEndpoints()
    {
        var client = CreateAuthenticatedClient("Employee");
        var response = await client.GetAsync("/api/admin/dashboard");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ManagerCannotAccessAdminEndpoints()
    {
        var client = CreateAuthenticatedClient("Manager");
        var response = await client.GetAsync("/api/admin/dashboard");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminCanAccessAdminEndpoints()
    {
        var client = CreateAuthenticatedClient("Admin");
        var response = await client.GetAsync("/api/admin/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousUsersReceive401OnProtectedEndpoints()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/admin/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private HttpClient CreateAuthenticatedClient(string role)
    {
        var client = _factory.CreateClient();
        var token = CreateJwtToken(role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string CreateJwtToken(string role)
    {
        var key = "THIS_IS_A_DEVELOPMENT_ONLY_SECRET_KEY_CHANGE_LATER_123456789";
        var issuer = "OpsFlow";
        var audience = "OpsFlow";
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, $"{role.ToLowerInvariant()}@example.com"),
            new Claim(ClaimTypes.Name, role),
            new Claim(ClaimTypes.Role, role)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
