using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;
using OpsFlow.Infrastructure.Persistence;
using Xunit;

namespace OpsFlow.Tests;

[Collection("SharedTestFactory")]
public class RequestDetailEndpointTests : IAsyncLifetime
{
    private readonly RequestAuditEndpointTests.TestFactory _factory;

    public RequestDetailEndpointTests(RequestAuditEndpointTests.TestFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task EmployeeCanViewOwnRequest()
    {
        var employee = NewUser("Employee", "employee.own.detail@test", UserRole.Employee);
        var manager = NewUser("Manager", "manager.own.detail@test", UserRole.Manager);
        var request = NewRequest(employee, manager, "Own request", RequestStatus.Draft);

        await ResetAndSeedAsync(new[] { employee, manager }, new[] { request });

        var client = _factory.CreateAuthenticatedClient(employee.Id, "Employee");
        var response = await client.GetAsync($"/api/requests/{request.Id}");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Own request", doc.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task EmployeeCannotViewAnotherEmployeesRequest()
    {
        var owner = NewUser("Owner", "owner.detail@test", UserRole.Employee);
        var otherEmployee = NewUser("Other", "other.detail@test", UserRole.Employee);
        var manager = NewUser("Manager", "manager.detail@test", UserRole.Manager);
        var request = NewRequest(owner, manager, "Private request", RequestStatus.Submitted);

        await ResetAndSeedAsync(new[] { owner, otherEmployee, manager }, new[] { request });

        var client = _factory.CreateAuthenticatedClient(otherEmployee.Id, "Employee");
        var response = await client.GetAsync($"/api/requests/{request.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ManagerCanViewAssignedRequest()
    {
        var owner = NewUser("Owner", "owner.assigned@test", UserRole.Employee);
        var manager = NewUser("Manager", "manager.assigned@test", UserRole.Manager);
        var request = NewRequest(owner, manager, "Assigned request", RequestStatus.Submitted);

        await ResetAndSeedAsync(new[] { owner, manager }, new[] { request });

        var client = _factory.CreateAuthenticatedClient(manager.Id, "Manager");
        var response = await client.GetAsync($"/api/requests/{request.Id}");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Assigned request", doc.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task AdminCanViewAnyRequest()
    {
        var owner = NewUser("Owner", "owner.admin@test", UserRole.Employee);
        var manager = NewUser("Manager", "manager.admin@test", UserRole.Manager);
        var admin = NewUser("Admin", "admin.any@test", UserRole.Admin);
        var request = NewRequest(owner, manager, "Admin view request", RequestStatus.Submitted);

        await ResetAndSeedAsync(new[] { owner, manager, admin }, new[] { request });

        var client = _factory.CreateAuthenticatedClient(admin.Id, "Admin");
        var response = await client.GetAsync($"/api/requests/{request.Id}");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Admin view request", doc.RootElement.GetProperty("title").GetString());
    }

    private async Task ResetAndSeedAsync(IEnumerable<User> users, IEnumerable<Request> requests)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        dbContext.Users.AddRange(users);
        dbContext.Requests.AddRange(requests);
        await dbContext.SaveChangesAsync();
    }

    private static User NewUser(string name, string email, UserRole role)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Request NewRequest(User owner, User manager, string title, RequestStatus status)
    {
        return new Request
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = title,
            Category = RequestCategory.Other,
            Status = status,
            CreatedByUserId = owner.Id,
            CreatedByUser = owner,
            AssignedReviewerId = manager.Id,
            AssignedReviewer = manager,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
