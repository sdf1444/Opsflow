using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;
using OpsFlow.Infrastructure.Persistence;
using Xunit;

namespace OpsFlow.Tests;

[Collection("SharedTestFactory")]
public class ApprovalQueueEndpointTests : IAsyncLifetime
{
    private readonly RequestAuditEndpointTests.TestFactory _factory;

    public ApprovalQueueEndpointTests(RequestAuditEndpointTests.TestFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ManagerQueueLoads_OnlyAssignedRequestsAppear()
    {
        var manager = NewUser("Manager", "manager.queue@test", UserRole.Manager);
        var anotherManager = NewUser("Another", "another.manager@test", UserRole.Manager);
        var owner = NewUser("Employee", "employee.queue@test", UserRole.Employee);

        var assigned = NewRequest(owner, manager, "Assigned pending", RequestStatus.Submitted, DateTime.UtcNow.AddHours(-1));
        var unassigned = NewRequest(owner, anotherManager, "Not assigned", RequestStatus.Submitted, DateTime.UtcNow.AddHours(-1));
        var approved = NewRequest(owner, manager, "Already approved", RequestStatus.Approved, DateTime.UtcNow.AddHours(-1));

        await ResetAndSeedAsync(new[] { manager, anotherManager, owner }, new[] { assigned, unassigned, approved });

        var client = _factory.CreateAuthenticatedClient(manager.Id, "Manager");
        var response = await client.GetAsync("/api/approvals?page=1&pageSize=10&sortBy=updatedAt&sortDirection=desc");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("items");
        Assert.Single(items.EnumerateArray());
        Assert.Equal("Assigned pending", items[0].GetProperty("title").GetString());
        Assert.Equal(1, doc.RootElement.GetProperty("summary").GetProperty("pending").GetInt32());
    }

    [Fact]
    public async Task AdminQueueLoads_AllPendingRequestsAppear()
    {
        var admin = NewUser("Admin", "admin.queue@test", UserRole.Admin);
        var managerA = NewUser("Manager A", "manager.a@test", UserRole.Manager);
        var managerB = NewUser("Manager B", "manager.b@test", UserRole.Manager);
        var ownerA = NewUser("Owner A", "owner.a@test", UserRole.Employee);
        var ownerB = NewUser("Owner B", "owner.b@test", UserRole.Employee);

        var pendingA = NewRequest(ownerA, managerA, "Pending A", RequestStatus.Submitted, DateTime.UtcNow.AddHours(-3));
        var pendingB = NewRequest(ownerB, managerB, "Pending B", RequestStatus.UnderReview, DateTime.UtcNow.AddHours(-2));
        var nonPending = NewRequest(ownerA, managerA, "Approved", RequestStatus.Approved, DateTime.UtcNow.AddHours(-1));

        await ResetAndSeedAsync(new[] { admin, managerA, managerB, ownerA, ownerB }, new[] { pendingA, pendingB, nonPending });

        var client = _factory.CreateAuthenticatedClient(admin.Id, "Admin");
        var response = await client.GetAsync("/api/approvals?page=1&pageSize=10");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, doc.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, doc.RootElement.GetProperty("summary").GetProperty("pending").GetInt32());
    }

    [Fact]
    public async Task EmployeeCannotAccessApprovalQueue()
    {
        var employee = NewUser("Employee", "employee.forbidden@test", UserRole.Employee);

        await ResetAndSeedAsync(new[] { employee }, Array.Empty<Request>());

        var client = _factory.CreateAuthenticatedClient(employee.Id, "Employee");
        var response = await client.GetAsync("/api/approvals");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ApprovingRequest_RemovesItFromManagerQueue()
    {
        var manager = NewUser("Manager", "manager.approve@test", UserRole.Manager);
        var owner = NewUser("Employee", "employee.approve@test", UserRole.Employee);
        var request = NewRequest(owner, manager, "Approve me", RequestStatus.Submitted, DateTime.UtcNow.AddHours(-1));

        await ResetAndSeedAsync(new[] { manager, owner }, new[] { request });

        var client = _factory.CreateAuthenticatedClient(manager.Id, "Manager");

        var before = await client.GetAsync("/api/approvals");
        before.EnsureSuccessStatusCode();
        using (var beforeDoc = JsonDocument.Parse(await before.Content.ReadAsStringAsync()))
        {
            Assert.Equal(1, beforeDoc.RootElement.GetProperty("totalCount").GetInt32());
        }

        var approveResponse = await client.PostAsync($"/api/requests/{request.Id}/approve", null);
        approveResponse.EnsureSuccessStatusCode();

        var after = await client.GetAsync("/api/approvals");
        after.EnsureSuccessStatusCode();
        using var afterDoc = JsonDocument.Parse(await after.Content.ReadAsStringAsync());
        Assert.Equal(0, afterDoc.RootElement.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task RejectingRequest_RemovesItFromManagerQueue()
    {
        var manager = NewUser("Manager", "manager.reject@test", UserRole.Manager);
        var owner = NewUser("Employee", "employee.reject@test", UserRole.Employee);
        var request = NewRequest(owner, manager, "Reject me", RequestStatus.Submitted, DateTime.UtcNow.AddHours(-1));

        await ResetAndSeedAsync(new[] { manager, owner }, new[] { request });

        var client = _factory.CreateAuthenticatedClient(manager.Id, "Manager");

        var before = await client.GetAsync("/api/approvals");
        before.EnsureSuccessStatusCode();
        using (var beforeDoc = JsonDocument.Parse(await before.Content.ReadAsStringAsync()))
        {
            Assert.Equal(1, beforeDoc.RootElement.GetProperty("totalCount").GetInt32());
        }

        var payload = JsonSerializer.Serialize(new { reason = "Needs more budget detail" });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var rejectResponse = await client.PostAsync($"/api/requests/{request.Id}/reject", content);
        rejectResponse.EnsureSuccessStatusCode();

        var after = await client.GetAsync("/api/approvals");
        after.EnsureSuccessStatusCode();
        using var afterDoc = JsonDocument.Parse(await after.Content.ReadAsStringAsync());
        Assert.Equal(0, afterDoc.RootElement.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task AdminSummaryUpdatesAfterApproval()
    {
        var admin = NewUser("Admin", "admin.summary@test", UserRole.Admin);
        var manager = NewUser("Manager", "manager.summary@test", UserRole.Manager);
        var owner = NewUser("Employee", "employee.summary@test", UserRole.Employee);

        var oldPending = NewRequest(owner, manager, "Old pending", RequestStatus.Submitted, DateTime.UtcNow.AddDays(-3));
        oldPending.SubmittedAt = DateTime.UtcNow.AddDays(-3);
        var freshPending = NewRequest(owner, manager, "Fresh pending", RequestStatus.Submitted, DateTime.UtcNow.AddHours(-2));

        await ResetAndSeedAsync(new[] { admin, manager, owner }, new[] { oldPending, freshPending });

        var client = _factory.CreateAuthenticatedClient(admin.Id, "Admin");

        var before = await client.GetAsync("/api/approvals");
        before.EnsureSuccessStatusCode();
        using (var beforeDoc = JsonDocument.Parse(await before.Content.ReadAsStringAsync()))
        {
            Assert.Equal(2, beforeDoc.RootElement.GetProperty("summary").GetProperty("pending").GetInt32());
            Assert.True(beforeDoc.RootElement.GetProperty("summary").GetProperty("overdue").GetInt32() >= 1);
        }

        var approveResponse = await client.PostAsync($"/api/requests/{oldPending.Id}/approve", null);
        approveResponse.EnsureSuccessStatusCode();

        var after = await client.GetAsync("/api/approvals");
        after.EnsureSuccessStatusCode();
        using var afterDoc = JsonDocument.Parse(await after.Content.ReadAsStringAsync());
        Assert.Equal(1, afterDoc.RootElement.GetProperty("summary").GetProperty("pending").GetInt32());
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

    private static Request NewRequest(User owner, User manager, string title, RequestStatus status, DateTime updatedAt)
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
            CreatedAt = updatedAt.AddMinutes(-20),
            UpdatedAt = updatedAt,
            SubmittedAt = status == RequestStatus.Submitted || status == RequestStatus.UnderReview ? updatedAt.AddMinutes(-5) : null
        };
    }
}
