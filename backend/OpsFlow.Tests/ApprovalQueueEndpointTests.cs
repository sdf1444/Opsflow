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
    public async Task SubmittingRequest_AssignsLeastLoadedManagerAndAllowsApproval()
    {
        var managerA = NewUser("Manager A", "manager.a.auto@test", UserRole.Manager);
        var managerB = NewUser("Manager B", "manager.b.auto@test", UserRole.Manager);
        var owner = NewUser("Employee", "employee.auto@test", UserRole.Employee);
        var existingRequest = NewRequest(owner, managerA, "Existing pending", RequestStatus.Submitted, DateTime.UtcNow.AddHours(-1));

        await ResetAndSeedAsync(new[] { managerA, managerB, owner }, new[] { existingRequest });

        var employeeClient = _factory.CreateAuthenticatedClient(owner.Id, "Employee");
        var payload = JsonSerializer.Serialize(new
        {
            title = "New submitted request",
            description = "A submitted request for manager review",
            category = "Equipment",
            submit = true
        });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var createResponse = await employeeClient.PostAsync("/api/requests", content);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        using var created = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var requestId = created.RootElement.GetProperty("id").GetGuid();
        Assert.Equal(managerB.Id, created.RootElement.GetProperty("assignedReviewerId").GetGuid());

        var managerClient = _factory.CreateAuthenticatedClient(managerB.Id, "Manager");
        var queueResponse = await managerClient.GetAsync("/api/approvals");
        queueResponse.EnsureSuccessStatusCode();
        using (var queue = JsonDocument.Parse(await queueResponse.Content.ReadAsStringAsync()))
        {
            Assert.Equal(1, queue.RootElement.GetProperty("totalCount").GetInt32());
            Assert.Equal("New submitted request", queue.RootElement.GetProperty("items")[0].GetProperty("title").GetString());
        }

        var detailResponse = await managerClient.GetAsync($"/api/requests/{requestId}");
        detailResponse.EnsureSuccessStatusCode();

        var approveResponse = await managerClient.PostAsync($"/api/requests/{requestId}/approve", null);
        approveResponse.EnsureSuccessStatusCode();
        using var approved = JsonDocument.Parse(await approveResponse.Content.ReadAsStringAsync());
        Assert.Equal("Approved", approved.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task SubmittingRequestWithoutManagers_ReturnsBadRequest()
    {
        var owner = NewUser("Employee", "employee.no-manager@test", UserRole.Employee);
        await ResetAndSeedAsync(new[] { owner }, Array.Empty<Request>());

        var client = _factory.CreateAuthenticatedClient(owner.Id, "Employee");
        var payload = JsonSerializer.Serialize(new
        {
            title = "Cannot route request",
            description = "No manager exists to review this request",
            category = "Equipment",
            submit = true
        });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/requests", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains("No managers", body.RootElement.GetProperty("detail").GetString());

        var requestsResponse = await client.GetAsync("/api/requests");
        requestsResponse.EnsureSuccessStatusCode();
        using var requests = JsonDocument.Parse(await requestsResponse.Content.ReadAsStringAsync());
        Assert.Equal(0, requests.RootElement.GetProperty("totalCount").GetInt32());

        payload = JsonSerializer.Serialize(new
        {
            title = "Draft awaiting a manager",
            description = "This draft can be submitted after a manager is available",
            category = "Equipment",
            submit = false
        });
        using var draftContent = new StringContent(payload, Encoding.UTF8, "application/json");
        var draftResponse = await client.PostAsync("/api/requests", draftContent);
        Assert.Equal(HttpStatusCode.Created, draftResponse.StatusCode);

        using var draft = JsonDocument.Parse(await draftResponse.Content.ReadAsStringAsync());
        var draftId = draft.RootElement.GetProperty("id").GetGuid();
        var submitResponse = await client.PostAsync($"/api/requests/{draftId}/submit", null);
        Assert.Equal(HttpStatusCode.BadRequest, submitResponse.StatusCode);

        var draftDetail = await client.GetAsync($"/api/requests/{draftId}");
        draftDetail.EnsureSuccessStatusCode();
        using var detail = JsonDocument.Parse(await draftDetail.Content.ReadAsStringAsync());
        Assert.Equal("Draft", detail.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task SubmittingExistingDraft_AssignsManagerAndMakesItReviewable()
    {
        var manager = NewUser("Manager", "manager.draft-submit@test", UserRole.Manager);
        var owner = NewUser("Employee", "employee.draft-submit@test", UserRole.Employee);
        var draft = NewRequest(owner, manager, "Existing draft", RequestStatus.Draft, DateTime.UtcNow);
        draft.AssignedReviewerId = null;
        draft.AssignedReviewer = null;

        await ResetAndSeedAsync(new[] { manager, owner }, new[] { draft });

        var employeeClient = _factory.CreateAuthenticatedClient(owner.Id, "Employee");
        var submitResponse = await employeeClient.PostAsync($"/api/requests/{draft.Id}/submit", null);
        submitResponse.EnsureSuccessStatusCode();
        using var submitted = JsonDocument.Parse(await submitResponse.Content.ReadAsStringAsync());
        Assert.Equal(manager.Id, submitted.RootElement.GetProperty("assignedReviewerId").GetGuid());
        Assert.Equal("Submitted", submitted.RootElement.GetProperty("status").GetString());

        var managerClient = _factory.CreateAuthenticatedClient(manager.Id, "Manager");
        var queueResponse = await managerClient.GetAsync("/api/approvals");
        queueResponse.EnsureSuccessStatusCode();
        using var queue = JsonDocument.Parse(await queueResponse.Content.ReadAsStringAsync());
        Assert.Equal(1, queue.RootElement.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task PendingEndpointOnlyReturnsRequestsAssignedToManager()
    {
        var manager = NewUser("Manager", "manager.pending-filter@test", UserRole.Manager);
        var anotherManager = NewUser("Another", "another.pending-filter@test", UserRole.Manager);
        var owner = NewUser("Employee", "employee.pending-filter@test", UserRole.Employee);
        var assigned = NewRequest(owner, manager, "Assigned pending", RequestStatus.Submitted, DateTime.UtcNow.AddHours(-1));
        var otherAssigned = NewRequest(owner, anotherManager, "Other manager pending", RequestStatus.Submitted, DateTime.UtcNow.AddHours(-1));
        otherAssigned.AssignedReviewerId = anotherManager.Id;
        otherAssigned.AssignedReviewer = anotherManager;

        await ResetAndSeedAsync(new[] { manager, anotherManager, owner }, new[] { assigned, otherAssigned });

        var client = _factory.CreateAuthenticatedClient(manager.Id, "Manager");
        var response = await client.GetAsync("/api/requests/pending");

        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Single(doc.RootElement.EnumerateArray());
        Assert.Equal("Assigned pending", doc.RootElement[0].GetProperty("title").GetString());
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
