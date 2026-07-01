using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using OpsFlow.Api;
using OpsFlow.Application.DTOs.Requests;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;
using OpsFlow.Infrastructure.Persistence;
using Xunit;

namespace OpsFlow.Tests;

public class RequestAuditEndpointTests : IClassFixture<RequestAuditEndpointTests.TestFactory>
{
    private readonly TestFactory _factory;

    public RequestAuditEndpointTests(TestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OwnerCanGetAuditTimeline()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = "employee@test",
            Role = UserRole.Employee
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Audit title",
            Description = "Audit description",
            Category = RequestCategory.SoftwareAccess,
            Status = RequestStatus.Draft,
            CreatedByUserId = user.Id,
            CreatedByUser = user,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        request.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            UserId = user.Id,
            Action = "RequestCreated",
            Description = "Created request draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Users.Add(user);
        dbContext.Requests.Add(request);
        await dbContext.SaveChangesAsync();

        var client = _factory.CreateAuthenticatedClient(user.Id, "Employee");

        var response = await client.GetAsync($"/api/requests/{request.Id}/audit");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        var timeline = JsonSerializer.Deserialize<List<AuditLogDto>>(payload, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(timeline);
        Assert.Single(timeline);
        Assert.Equal("RequestCreated", timeline![0].Action);
        Assert.Equal(user.Id, timeline[0].UserId);
        Assert.Equal(request.Id, timeline[0].RequestId);
        Assert.Equal("Created request draft", timeline[0].Description);
    }

    [Fact]
    public async Task ManagerCanGetAuditTimelineForAnyRequest()
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = "employee@test",
            Role = UserRole.Employee
        };

        var manager = new User
        {
            Id = Guid.NewGuid(),
            Name = "Manager",
            Email = "manager@test",
            Role = UserRole.Manager
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Audit title",
            Description = "Audit description",
            Category = RequestCategory.SoftwareAccess,
            Status = RequestStatus.Draft,
            CreatedByUserId = owner.Id,
            CreatedByUser = owner,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        request.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            UserId = owner.Id,
            Action = "RequestCreated",
            Description = "Created request draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Users.Add(owner);
        dbContext.Users.Add(manager);
        dbContext.Requests.Add(request);
        await dbContext.SaveChangesAsync();

        var client = _factory.CreateAuthenticatedClient(manager.Id, "Manager");
        var response = await client.GetAsync($"/api/requests/{request.Id}/audit");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        var timeline = JsonSerializer.Deserialize<List<AuditLogDto>>(payload, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(timeline);
        Assert.Single(timeline);
        Assert.Equal("RequestCreated", timeline![0].Action);
        Assert.Equal(owner.Id, timeline[0].UserId);
        Assert.Equal(request.Id, timeline[0].RequestId);
    }

    [Fact]
    public async Task AdminCanGetAuditTimelineForAnyRequest()
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = "employee@test",
            Role = UserRole.Employee
        };

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@test",
            Role = UserRole.Admin
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Audit title",
            Description = "Audit description",
            Category = RequestCategory.SoftwareAccess,
            Status = RequestStatus.Draft,
            CreatedByUserId = owner.Id,
            CreatedByUser = owner,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        request.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            UserId = owner.Id,
            Action = "RequestCreated",
            Description = "Created request draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Users.Add(owner);
        dbContext.Users.Add(admin);
        dbContext.Requests.Add(request);
        await dbContext.SaveChangesAsync();

        var client = _factory.CreateAuthenticatedClient(admin.Id, "Admin");
        var response = await client.GetAsync($"/api/requests/{request.Id}/audit");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        var timeline = JsonSerializer.Deserialize<List<AuditLogDto>>(payload, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(timeline);
        Assert.Single(timeline);
        Assert.Equal("RequestCreated", timeline![0].Action);
        Assert.Equal(owner.Id, timeline[0].UserId);
        Assert.Equal(request.Id, timeline[0].RequestId);
    }

    [Fact]
    public async Task UnauthorizedUserCannotGetAuditTimeline()
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = "employee@test",
            Role = UserRole.Employee
        };

        var otherUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Other Employee",
            Email = "other@test",
            Role = UserRole.Employee
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Audit title",
            Description = "Audit description",
            Category = RequestCategory.SoftwareAccess,
            Status = RequestStatus.Draft,
            CreatedByUserId = owner.Id,
            CreatedByUser = owner,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        request.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            UserId = owner.Id,
            Action = "RequestCreated",
            Description = "Created request draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Users.Add(owner);
        dbContext.Users.Add(otherUser);
        dbContext.Requests.Add(request);
        await dbContext.SaveChangesAsync();

        var client = _factory.CreateAuthenticatedClient(otherUser.Id, "Employee");
        var response = await client.GetAsync($"/api/requests/{request.Id}/audit");

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AuditTimelineReturnsNotFoundForMissingRequest()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = "employee@test",
            Role = UserRole.Employee
        };

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var client = _factory.CreateAuthenticatedClient(user.Id, "Employee");
        var response = await client.GetAsync($"/api/requests/{Guid.NewGuid()}/audit");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    public sealed class TestFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"OpsFlowTestDb-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.RemoveAll(typeof(DbContextOptions));
                services.RemoveAll(typeof(AppDbContext));

                services.AddScoped(_ =>
                {
                    var options = new DbContextOptionsBuilder<AppDbContext>()
                        .UseInMemoryDatabase(_databaseName)
                        .Options;

                    return new AppDbContext(options);
                });
            });
        }

        public HttpClient CreateAuthenticatedClient(Guid userId, string role)
        {
            var client = CreateClient();
            var token = CreateJwtToken(userId, role);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private static string CreateJwtToken(Guid userId, string role)
        {
            var key = "THIS_IS_A_DEVELOPMENT_ONLY_SECRET_KEY_CHANGE_LATER_123456789";
            var issuer = "OpsFlow";
            var audience = "OpsFlow";
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, "employee@test"),
                new Claim(ClaimTypes.Name, "Employee"),
                new Claim(ClaimTypes.Role, role)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
