using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;
using OpsFlow.Infrastructure.Persistence;

namespace OpsFlow.Tests;

public class DashboardEndpointTests : IClassFixture<RequestAuditEndpointTests.TestFactory>
{
    private readonly RequestAuditEndpointTests.TestFactory _factory;

    public DashboardEndpointTests(RequestAuditEndpointTests.TestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task EmployeeOnlySeesOwnRequests()
    {
        var employee = new User
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = "employee@test",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var other = new User
        {
            Id = Guid.NewGuid(),
            Name = "Other",
            Email = "other@test",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await ResetAndSeedAsync(db =>
        {
            db.Users.Add(employee);
            db.Users.Add(other);
            db.Requests.Add(NewRequest(employee, "Mine Draft", RequestStatus.Draft, DateTime.UtcNow.AddMinutes(-20)));
            db.Requests.Add(NewRequest(employee, "Mine Approved", RequestStatus.Approved, DateTime.UtcNow.AddMinutes(-10)));
            db.Requests.Add(NewRequest(other, "Not Mine", RequestStatus.Rejected, DateTime.UtcNow));
        });

        var client = _factory.CreateAuthenticatedClient(employee.Id, "Employee");
        var response = await client.GetAsync("/api/dashboard");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, doc.RootElement.GetProperty("draftCount").GetInt32());
        Assert.Equal(0, doc.RootElement.GetProperty("submittedCount").GetInt32());
        Assert.Equal(1, doc.RootElement.GetProperty("approvedCount").GetInt32());
        Assert.Equal(0, doc.RootElement.GetProperty("pendingApprovalCount").GetInt32());
        Assert.Equal(0, doc.RootElement.GetProperty("rejectedCount").GetInt32());

        var recent = doc.RootElement.GetProperty("recentRequests");
        Assert.Equal(2, recent.GetArrayLength());
        Assert.All(recent.EnumerateArray(), x => Assert.Equal("Employee", x.GetProperty("createdBy").GetString()));
    }

    [Fact]
    public async Task ManagerOnlySeesAssignedRequests()
    {
        var employee = new User
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = "employee@test",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var manager = new User
        {
            Id = Guid.NewGuid(),
            Name = "Manager",
            Email = "manager@test",
            Role = UserRole.Manager,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var otherManager = new User
        {
            Id = Guid.NewGuid(),
            Name = "Other Manager",
            Email = "othermanager@test",
            Role = UserRole.Manager,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await ResetAndSeedAsync(db =>
        {
            db.Users.Add(employee);
            db.Users.Add(manager);
            db.Users.Add(otherManager);

            var r1 = NewRequest(employee, "Assigned Submitted", RequestStatus.Submitted, DateTime.UtcNow.AddMinutes(-30));
            r1.AssignedReviewerId = manager.Id;

            var r2 = NewRequest(employee, "Assigned Approved", RequestStatus.Approved, DateTime.UtcNow.AddMinutes(-20));
            r2.AssignedReviewerId = manager.Id;

            var r3 = NewRequest(employee, "Assigned UnderReview", RequestStatus.UnderReview, DateTime.UtcNow.AddMinutes(-10));
            r3.AssignedReviewerId = manager.Id;

            var r4 = NewRequest(employee, "Not Assigned", RequestStatus.Rejected, DateTime.UtcNow);
            r4.AssignedReviewerId = otherManager.Id;

            db.Requests.AddRange(r1, r2, r3, r4);
        });

        var client = _factory.CreateAuthenticatedClient(manager.Id, "Manager");
        var response = await client.GetAsync("/api/dashboard");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, doc.RootElement.GetProperty("pendingApprovalCount").GetInt32());
        Assert.Equal(1, doc.RootElement.GetProperty("approvedCount").GetInt32());
        Assert.Equal(0, doc.RootElement.GetProperty("rejectedCount").GetInt32());
        Assert.Equal(3, doc.RootElement.GetProperty("totalAssignedCount").GetInt32());

        var recent = doc.RootElement.GetProperty("recentRequests");
        Assert.Equal(3, recent.GetArrayLength());
    }

    [Fact]
    public async Task AdminSeesAllRequestsAndGlobalCounts()
    {
        var employee = new User
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = "employee@test",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var manager = new User
        {
            Id = Guid.NewGuid(),
            Name = "Manager",
            Email = "manager@test",
            Role = UserRole.Manager,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@test",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await ResetAndSeedAsync(db =>
        {
            db.Users.Add(employee);
            db.Users.Add(manager);
            db.Users.Add(admin);

            var req1 = NewRequest(employee, "Draft", RequestStatus.Draft, DateTime.UtcNow.AddMinutes(-30));
            var req2 = NewRequest(employee, "Submitted", RequestStatus.Submitted, DateTime.UtcNow.AddMinutes(-20));
            var req3 = NewRequest(manager, "Approved", RequestStatus.Approved, DateTime.UtcNow.AddMinutes(-10));
            var req4 = NewRequest(manager, "Rejected", RequestStatus.Rejected, DateTime.UtcNow);

            db.Requests.AddRange(req1, req2, req3, req4);
            db.RequestComments.Add(new RequestComment
            {
                Id = Guid.NewGuid(),
                RequestId = req1.Id,
                UserId = employee.Id,
                Body = "Comment",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            db.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                RequestId = req1.Id,
                UserId = employee.Id,
                Action = "RequestCreated",
                Description = "Created request.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        });

        var client = _factory.CreateAuthenticatedClient(admin.Id, "Admin");
        var response = await client.GetAsync("/api/dashboard");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, doc.RootElement.GetProperty("draftCount").GetInt32());
        Assert.Equal(1, doc.RootElement.GetProperty("submittedCount").GetInt32());
        Assert.Equal(1, doc.RootElement.GetProperty("approvedCount").GetInt32());
        Assert.Equal(1, doc.RootElement.GetProperty("rejectedCount").GetInt32());
        Assert.Equal(1, doc.RootElement.GetProperty("pendingApprovalCount").GetInt32());

        Assert.Equal(3, doc.RootElement.GetProperty("totalUsers").GetInt32());
        Assert.Equal(4, doc.RootElement.GetProperty("totalRequests").GetInt32());
        Assert.Equal(1, doc.RootElement.GetProperty("totalComments").GetInt32());
        Assert.Equal(1, doc.RootElement.GetProperty("totalAuditEntries").GetInt32());
        Assert.Equal(4, doc.RootElement.GetProperty("recentRequests").GetArrayLength());
    }

    [Fact]
    public async Task RecentRequests_AreOrderedByUpdatedAtDescending()
    {
        var employee = new User
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = "employee@test",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await ResetAndSeedAsync(db =>
        {
            db.Users.Add(employee);
            db.Requests.Add(NewRequest(employee, "Old", RequestStatus.Draft, DateTime.UtcNow.AddMinutes(-30)));
            db.Requests.Add(NewRequest(employee, "New", RequestStatus.Submitted, DateTime.UtcNow.AddMinutes(-10)));
            db.Requests.Add(NewRequest(employee, "Newest", RequestStatus.Approved, DateTime.UtcNow));
        });

        var client = _factory.CreateAuthenticatedClient(employee.Id, "Employee");
        var response = await client.GetAsync("/api/dashboard");

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var recent = doc.RootElement.GetProperty("recentRequests");
        Assert.Equal("Newest", recent[0].GetProperty("title").GetString());
        Assert.Equal("New", recent[1].GetProperty("title").GetString());
        Assert.Equal("Old", recent[2].GetProperty("title").GetString());
    }

    [Fact]
    public async Task AnonymousUser_IsRejected()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/dashboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task ResetAndSeedAsync(Action<AppDbContext> seed)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        seed(dbContext);
        await dbContext.SaveChangesAsync();
    }

    private static Request NewRequest(User createdBy, string title, RequestStatus status, DateTime updatedAt)
    {
        return new Request
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = title,
            Category = RequestCategory.Other,
            Status = status,
            CreatedByUserId = createdBy.Id,
            CreatedByUser = createdBy,
            CreatedAt = updatedAt.AddMinutes(-2),
            UpdatedAt = updatedAt
        };
    }
}
