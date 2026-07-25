using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;
using OpsFlow.Infrastructure.Persistence;
using Xunit;

namespace OpsFlow.Tests;

public class RequestListEndpointTests : IClassFixture<RequestAuditEndpointTests.TestFactory>, IAsyncLifetime
{
    private readonly RequestAuditEndpointTests.TestFactory _factory;

    public RequestListEndpointTests(RequestAuditEndpointTests.TestFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetRequests_SupportsPaginationMetadata()
    {
        var manager = NewUser("Manager", "manager.pagination@test", UserRole.Manager);
        var owner = NewUser("Employee", "employee.pagination@test", UserRole.Employee);

        var now = DateTime.UtcNow;
        var requests = new List<Request>
        {
            NewRequest(owner, manager, "Page-1", RequestStatus.Draft, RequestCategory.Equipment, now.AddMinutes(1)),
            NewRequest(owner, manager, "Page-2", RequestStatus.Draft, RequestCategory.Equipment, now.AddMinutes(2)),
            NewRequest(owner, manager, "Page-3", RequestStatus.Draft, RequestCategory.Equipment, now.AddMinutes(3)),
            NewRequest(owner, manager, "Page-4", RequestStatus.Draft, RequestCategory.Equipment, now.AddMinutes(4)),
            NewRequest(owner, manager, "Page-5", RequestStatus.Draft, RequestCategory.Equipment, now.AddMinutes(5))
        };

        await ResetAndSeedAsync(new[] { manager, owner }, requests);

        var client = _factory.CreateAuthenticatedClient(manager.Id, "Manager");
        var response = await client.GetAsync("/api/requests?page=2&pageSize=2&category=Equipment&sortBy=updatedAt&sortDirection=desc");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, doc.RootElement.GetProperty("page").GetInt32());
        Assert.Equal(2, doc.RootElement.GetProperty("pageSize").GetInt32());
        Assert.Equal(5, doc.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, doc.RootElement.GetProperty("totalPages").GetInt32());

        var items = doc.RootElement.GetProperty("items");
        Assert.Equal(2, items.GetArrayLength());
        Assert.Equal("Page-3", items[0].GetProperty("title").GetString());
        Assert.Equal("Page-2", items[1].GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetRequests_SupportsStatusFiltering()
    {
        var manager = NewUser("Manager", "manager.filter@test", UserRole.Manager);
        var owner = NewUser("Employee", "employee.filter@test", UserRole.Employee);

        var requests = new List<Request>
        {
            NewRequest(owner, manager, "Submitted-1", RequestStatus.Submitted, RequestCategory.Other, DateTime.UtcNow.AddMinutes(1)),
            NewRequest(owner, manager, "Submitted-2", RequestStatus.Submitted, RequestCategory.Other, DateTime.UtcNow.AddMinutes(2)),
            NewRequest(owner, manager, "Draft-1", RequestStatus.Draft, RequestCategory.Other, DateTime.UtcNow.AddMinutes(3))
        };

        await ResetAndSeedAsync(new[] { manager, owner }, requests);

        var client = _factory.CreateAuthenticatedClient(manager.Id, "Manager");
        var response = await client.GetAsync("/api/requests?page=1&pageSize=20&status=Submitted&category=Other&sortBy=updatedAt&sortDirection=desc");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, doc.RootElement.GetProperty("totalCount").GetInt32());

        var items = doc.RootElement.GetProperty("items");
        Assert.Equal(2, items.GetArrayLength());

        foreach (var item in items.EnumerateArray())
        {
            Assert.Equal("Submitted", item.GetProperty("status").GetString());
        }
    }

    [Fact]
    public async Task GetRequests_SupportsSorting()
    {
        var manager = NewUser("Manager", "manager.sort@test", UserRole.Manager);
        var owner = NewUser("Employee", "employee.sort@test", UserRole.Employee);

        var requests = new List<Request>
        {
            NewRequest(owner, manager, "Newest", RequestStatus.Draft, RequestCategory.Training, DateTime.UtcNow.AddMinutes(3)),
            NewRequest(owner, manager, "Middle", RequestStatus.Draft, RequestCategory.Training, DateTime.UtcNow.AddMinutes(2)),
            NewRequest(owner, manager, "Oldest", RequestStatus.Draft, RequestCategory.Training, DateTime.UtcNow.AddMinutes(1))
        };

        await ResetAndSeedAsync(new[] { manager, owner }, requests);

        var client = _factory.CreateAuthenticatedClient(manager.Id, "Manager");
        var response = await client.GetAsync("/api/requests?page=1&pageSize=10&category=Training&sortBy=updatedAt&sortDirection=asc");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("items");
        Assert.Equal(3, items.GetArrayLength());
        Assert.Equal("Oldest", items[0].GetProperty("title").GetString());
        Assert.Equal("Middle", items[1].GetProperty("title").GetString());
        Assert.Equal("Newest", items[2].GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetRequests_SupportsSearchFiltering()
    {
        var manager = NewUser("Manager", "manager.search@test", UserRole.Manager);
        var owner = NewUser("Employee", "employee.search@test", UserRole.Employee);

        var requests = new List<Request>
        {
            NewRequest(owner, manager, "Laptop replacement", RequestStatus.Draft, RequestCategory.Equipment, DateTime.UtcNow.AddMinutes(1)),
            NewRequest(owner, manager, "Security training", RequestStatus.Draft, RequestCategory.Training, DateTime.UtcNow.AddMinutes(2)),
            NewRequest(owner, manager, "Chair replacement", RequestStatus.Draft, RequestCategory.Equipment, DateTime.UtcNow.AddMinutes(3))
        };

        requests[0].Description = "New laptop for frontend work";

        await ResetAndSeedAsync(new[] { manager, owner }, requests);

        var client = _factory.CreateAuthenticatedClient(manager.Id, "Manager");
        var response = await client.GetAsync("/api/requests?page=1&pageSize=10&search=laptop&sortBy=updatedAt&sortDirection=desc");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, doc.RootElement.GetProperty("totalCount").GetInt32());
        var items = doc.RootElement.GetProperty("items");
        Assert.Single(items.EnumerateArray());
        Assert.Equal("Laptop replacement", items[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetRequests_EmployeeSeesOwnRequestsOnly()
    {
        var employee = NewUser("Employee", "employee.own@test", UserRole.Employee);
        var manager = NewUser("Manager", "manager.own@test", UserRole.Manager);

        var employeeRequest = NewRequest(employee, manager, "Employee request", RequestStatus.Submitted, RequestCategory.Other, DateTime.UtcNow.AddMinutes(2));
        var managerRequest = NewRequest(manager, manager, "Manager request", RequestStatus.Submitted, RequestCategory.Other, DateTime.UtcNow.AddMinutes(1));

        await ResetAndSeedAsync(new[] { employee, manager }, new[] { employeeRequest, managerRequest });

        var client = _factory.CreateAuthenticatedClient(employee.Id, "Employee");
        var response = await client.GetAsync("/api/requests?page=1&pageSize=10&sortBy=updatedAt&sortDirection=desc");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, doc.RootElement.GetProperty("totalCount").GetInt32());
        var items = doc.RootElement.GetProperty("items");
        Assert.Single(items.EnumerateArray());
        Assert.Equal("Employee request", items[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetRequests_InvalidSortReturnsBadRequest()
    {
        var manager = NewUser("Manager", "manager.invalidsort@test", UserRole.Manager);

        await ResetAndSeedAsync(new[] { manager }, Array.Empty<Request>());

        var client = _factory.CreateAuthenticatedClient(manager.Id, "Manager");
        var response = await client.GetAsync("/api/requests?sortBy=not_supported&sortDirection=desc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Validation failed.", doc.RootElement.GetProperty("title").GetString());
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

    private static Request NewRequest(User owner, User manager, string title, RequestStatus status, RequestCategory category, DateTime updatedAt)
    {
        return new Request
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = title,
            Category = category,
            Status = status,
            CreatedByUserId = owner.Id,
            CreatedByUser = owner,
            AssignedReviewerId = manager.Id,
            AssignedReviewer = manager,
            CreatedAt = updatedAt.AddMinutes(-1),
            UpdatedAt = updatedAt
        };
    }
}
