using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;
using OpsFlow.Infrastructure.Persistence;
using Xunit;

namespace OpsFlow.Tests;

public class RequestCommentsEndpointTests : IClassFixture<RequestAuditEndpointTests.TestFactory>, IAsyncLifetime
{
    private readonly RequestAuditEndpointTests.TestFactory _factory;

    public RequestCommentsEndpointTests(RequestAuditEndpointTests.TestFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task OwnerCanAddComment_ReturnsCreated_AndCreatesAuditEntry()
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Name = "Owner User",
            Email = "owner@example.com",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Need equipment",
            Description = "Laptop replacement",
            Category = RequestCategory.Equipment,
            Status = RequestStatus.Draft,
            CreatedByUserId = owner.Id,
            CreatedByUser = owner,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Users.Add(owner);
            dbContext.Requests.Add(request);
            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(owner.Id, "Employee");
        var payload = JsonSerializer.Serialize(new { body = "Please provide a quotation before approval." });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"/api/requests/{request.Id}/comments", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        Assert.True(doc.RootElement.TryGetProperty("id", out var idProp));
        Assert.NotEqual(Guid.Empty, idProp.GetGuid());
        Assert.Equal("Owner User", doc.RootElement.GetProperty("authorName").GetString());
        Assert.Equal("owner@example.com", doc.RootElement.GetProperty("authorEmail").GetString());
        Assert.Equal("Please provide a quotation before approval.", doc.RootElement.GetProperty("body").GetString());
        Assert.True(doc.RootElement.TryGetProperty("createdAt", out _));

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var reloaded = await verificationDb.Requests.FindAsync(request.Id);
        Assert.NotNull(reloaded);

        var auditExists = verificationDb.AuditLogs.Any(a => a.RequestId == request.Id && a.Action == "CommentAdded");
        Assert.True(auditExists);
    }

    [Fact]
    public async Task GetComments_ReturnsChronologicalOrderWithAuthorInfo()
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Name = "Employee User",
            Email = "employee@example.com",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var manager = new User
        {
            Id = Guid.NewGuid(),
            Name = "Manager User",
            Email = "manager@example.com",
            Role = UserRole.Manager,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Travel expense",
            Description = "Conference attendance",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Submitted,
            CreatedByUserId = owner.Id,
            AssignedReviewerId = manager.Id,
            CreatedByUser = owner,
            AssignedReviewer = manager,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var first = new RequestComment
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            UserId = manager.Id,
            User = manager,
            Body = "Please provide a quotation before approval.",
            CreatedAt = DateTime.UtcNow.AddMinutes(-15),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-15)
        };

        var second = new RequestComment
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            UserId = owner.Id,
            User = owner,
            Body = "Quotation attached.",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Users.Add(owner);
            dbContext.Users.Add(manager);
            dbContext.Requests.Add(request);
            dbContext.RequestComments.Add(first);
            dbContext.RequestComments.Add(second);
            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(owner.Id, "Employee");
        var response = await client.GetAsync($"/api/requests/{request.Id}/comments");

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(2, doc.RootElement.GetArrayLength());

        var firstItem = doc.RootElement[0];
        var secondItem = doc.RootElement[1];

        Assert.Equal("Manager User", firstItem.GetProperty("authorName").GetString());
        Assert.Equal("manager@example.com", firstItem.GetProperty("authorEmail").GetString());
        Assert.Equal("Please provide a quotation before approval.", firstItem.GetProperty("body").GetString());

        Assert.Equal("Employee User", secondItem.GetProperty("authorName").GetString());
        Assert.Equal("employee@example.com", secondItem.GetProperty("authorEmail").GetString());
        Assert.Equal("Quotation attached.", secondItem.GetProperty("body").GetString());

        var firstCreated = firstItem.GetProperty("createdAt").GetDateTime();
        var secondCreated = secondItem.GetProperty("createdAt").GetDateTime();
        Assert.True(firstCreated <= secondCreated);
    }

    [Fact]
    public async Task ManagerCanAddCommentOnAssignedRequest()
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Name = "Employee User",
            Email = "employee@example.com",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var manager = new User
        {
            Id = Guid.NewGuid(),
            Name = "Manager User",
            Email = "manager@example.com",
            Role = UserRole.Manager,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Travel expense",
            Description = "Conference attendance",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Submitted,
            CreatedByUserId = owner.Id,
            AssignedReviewerId = manager.Id,
            CreatedByUser = owner,
            AssignedReviewer = manager,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Users.Add(owner);
            dbContext.Users.Add(manager);
            dbContext.Requests.Add(request);
            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(manager.Id, "Manager");
        var payload = JsonSerializer.Serialize(new { body = "Please attach receipts." });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"/api/requests/{request.Id}/comments", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AdminCanAddCommentOnAnyRequest()
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Name = "Employee User",
            Email = "employee@example.com",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin User",
            Email = "admin@example.com",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Software access",
            Description = "Need tool license",
            Category = RequestCategory.SoftwareAccess,
            Status = RequestStatus.Submitted,
            CreatedByUserId = owner.Id,
            CreatedByUser = owner,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Users.Add(owner);
            dbContext.Users.Add(admin);
            dbContext.Requests.Add(request);
            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(admin.Id, "Admin");
        var payload = JsonSerializer.Serialize(new { body = "Approved from policy perspective." });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"/api/requests/{request.Id}/comments", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AddComment_PersistsCommentDataCorrectly()
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Name = "Owner User",
            Email = "owner@example.com",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Equipment request",
            Description = "New monitor",
            Category = RequestCategory.Equipment,
            Status = RequestStatus.Draft,
            CreatedByUserId = owner.Id,
            CreatedByUser = owner,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        const string commentBody = "Please include model specifications.";

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Users.Add(owner);
            dbContext.Requests.Add(request);
            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(owner.Id, "Employee");
        var payload = JsonSerializer.Serialize(new { body = commentBody });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"/api/requests/{request.Id}/comments", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var savedComment = verificationDb.RequestComments
            .SingleOrDefault(c => c.RequestId == request.Id && c.UserId == owner.Id && c.Body == commentBody);

        Assert.NotNull(savedComment);
        Assert.Equal(commentBody, savedComment!.Body);
        Assert.Equal(request.Id, savedComment.RequestId);
        Assert.Equal(owner.Id, savedComment.UserId);
    }

    [Fact]
    public async Task AnonymousUsersReceive401OnCommentEndpoints()
    {
        var requestId = Guid.NewGuid();
        var client = _factory.CreateClient();

        var getResponse = await client.GetAsync($"/api/requests/{requestId}/comments");

        var payload = JsonSerializer.Serialize(new { body = "Test" });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var postResponse = await client.PostAsync($"/api/requests/{requestId}/comments", content);

        Assert.Equal(HttpStatusCode.Unauthorized, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, postResponse.StatusCode);
    }

    [Fact]
    public async Task EmployeeCannotCommentOnAnotherEmployeesRequest_ReturnsForbidden()
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Name = "Owner",
            Email = "owner@example.com",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var otherEmployee = new User
        {
            Id = Guid.NewGuid(),
            Name = "Other",
            Email = "other@example.com",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Budget request",
            Description = "Team activity",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Draft,
            CreatedByUserId = owner.Id,
            CreatedByUser = owner,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Users.Add(owner);
            dbContext.Users.Add(otherEmployee);
            dbContext.Requests.Add(request);
            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(otherEmployee.Id, "Employee");
        var payload = JsonSerializer.Serialize(new { body = "Attempting to comment" });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"/api/requests/{request.Id}/comments", content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddCommentWithEmptyBody_ReturnsBadRequest()
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Name = "Owner",
            Email = "owner@example.com",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Training request",
            Description = "External course",
            Category = RequestCategory.Training,
            Status = RequestStatus.Draft,
            CreatedByUserId = owner.Id,
            CreatedByUser = owner,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Users.Add(owner);
            dbContext.Requests.Add(request);
            await dbContext.SaveChangesAsync();
        }

        var client = _factory.CreateAuthenticatedClient(owner.Id, "Employee");
        var payload = JsonSerializer.Serialize(new { body = "   " });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"/api/requests/{request.Id}/comments", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseBody);

        Assert.Equal("Validation failed.", doc.RootElement.GetProperty("title").GetString());
        Assert.Equal("Comment is required.", doc.RootElement.GetProperty("detail").GetString());
    }
}
