using OpsFlow.Application.Interfaces;
using OpsFlow.Application.Services;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;
using System.Linq.Expressions;

namespace OpsFlow.Tests;

public class DashboardServiceTests
{
    [Fact]
    public async Task EmployeeDashboard_ReturnsOwnCountsAndRecentRequests()
    {
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var other = new User { Id = Guid.NewGuid(), Name = "Other", Email = "other@test", Role = UserRole.Employee };

        var requests = new List<Request>
        {
            NewRequest(employee, "Draft one", RequestStatus.Draft, DateTime.UtcNow.AddMinutes(-30)),
            NewRequest(employee, "Submitted one", RequestStatus.Submitted, DateTime.UtcNow.AddMinutes(-20)),
            NewRequest(employee, "Approved one", RequestStatus.Approved, DateTime.UtcNow.AddMinutes(-10)),
            NewRequest(employee, "Rejected one", RequestStatus.Rejected, DateTime.UtcNow.AddMinutes(-5)),
            NewRequest(other, "Other request", RequestStatus.Approved, DateTime.UtcNow)
        };

        var repo = new InMemoryDashboardRepository(
            requests,
            new[] { employee, other },
            commentsCount: 3,
            auditCount: 9);

        var service = new DashboardService(repo);

        var dto = await service.GetEmployeeDashboardAsync(employee.Id, CancellationToken.None);

        Assert.Equal(1, dto.DraftCount);
        Assert.Equal(1, dto.SubmittedCount);
        Assert.Equal(1, dto.PendingApprovalCount);
        Assert.Equal(1, dto.ApprovedCount);
        Assert.Equal(1, dto.RejectedCount);
        Assert.Equal(4, dto.RecentRequests.Count);
        Assert.Equal("Rejected one", dto.RecentRequests[0].Title);
        Assert.Equal("Employee", dto.RecentRequests[0].CreatedBy);
    }

    [Fact]
    public async Task ManagerDashboard_ReturnsAssignedStatsAndTotalAssigned()
    {
        var creator = new User { Id = Guid.NewGuid(), Name = "Creator", Email = "creator@test", Role = UserRole.Employee };
        var manager = new User { Id = Guid.NewGuid(), Name = "Manager", Email = "manager@test", Role = UserRole.Manager };
        var otherManager = new User { Id = Guid.NewGuid(), Name = "Other Manager", Email = "otherm@test", Role = UserRole.Manager };

        var assignedSubmitted = NewRequest(creator, "Submitted", RequestStatus.Submitted, DateTime.UtcNow.AddMinutes(-40));
        assignedSubmitted.AssignedReviewerId = manager.Id;

        var assignedApproved = NewRequest(creator, "Approved", RequestStatus.Approved, DateTime.UtcNow.AddMinutes(-30));
        assignedApproved.AssignedReviewerId = manager.Id;

        var assignedRejected = NewRequest(creator, "Rejected", RequestStatus.Rejected, DateTime.UtcNow.AddMinutes(-20));
        assignedRejected.AssignedReviewerId = manager.Id;

        var assignedUnderReview = NewRequest(creator, "UnderReview", RequestStatus.UnderReview, DateTime.UtcNow.AddMinutes(-10));
        assignedUnderReview.AssignedReviewerId = manager.Id;

        var notAssigned = NewRequest(creator, "Not assigned", RequestStatus.Submitted, DateTime.UtcNow);
        notAssigned.AssignedReviewerId = otherManager.Id;

        var repo = new InMemoryDashboardRepository(
            new[] { assignedSubmitted, assignedApproved, assignedRejected, assignedUnderReview, notAssigned },
            new[] { creator, manager, otherManager },
            commentsCount: 0,
            auditCount: 0);

        var service = new DashboardService(repo);

        var dto = await service.GetManagerDashboardAsync(manager.Id, CancellationToken.None);

        Assert.Equal(2, dto.PendingApprovalCount);
        Assert.Equal(1, dto.ApprovedCount);
        Assert.Equal(1, dto.RejectedCount);
        Assert.Equal(4, dto.TotalAssignedCount);
        Assert.Equal(4, dto.RecentRequests.Count);
        Assert.Equal("UnderReview", dto.RecentRequests[0].Title);
    }

    [Fact]
    public async Task AdminDashboard_ReturnsGlobalCountsAndRecentRequests()
    {
        var userA = new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@test", Role = UserRole.Employee };
        var userB = new User { Id = Guid.NewGuid(), Name = "Bob", Email = "bob@test", Role = UserRole.Manager };

        var requests = new[]
        {
            NewRequest(userA, "Draft", RequestStatus.Draft, DateTime.UtcNow.AddMinutes(-50)),
            NewRequest(userA, "Submitted", RequestStatus.Submitted, DateTime.UtcNow.AddMinutes(-40)),
            NewRequest(userB, "Approved", RequestStatus.Approved, DateTime.UtcNow.AddMinutes(-30)),
            NewRequest(userB, "Rejected", RequestStatus.Rejected, DateTime.UtcNow.AddMinutes(-20)),
            NewRequest(userB, "UnderReview", RequestStatus.UnderReview, DateTime.UtcNow.AddMinutes(-10))
        };

        var repo = new InMemoryDashboardRepository(
            requests,
            new[] { userA, userB },
            commentsCount: 7,
            auditCount: 13);

        var service = new DashboardService(repo);

        var dto = await service.GetAdminDashboardAsync(CancellationToken.None);

        Assert.Equal(1, dto.DraftCount);
        Assert.Equal(1, dto.SubmittedCount);
        Assert.Equal(2, dto.PendingApprovalCount);
        Assert.Equal(1, dto.ApprovedCount);
        Assert.Equal(1, dto.RejectedCount);
        Assert.Equal(2, dto.TotalUsers);
        Assert.Equal(5, dto.TotalRequests);
        Assert.Equal(7, dto.TotalComments);
        Assert.Equal(13, dto.TotalAuditEntries);
        Assert.Equal(5, dto.RecentRequests.Count);
        Assert.Equal("UnderReview", dto.RecentRequests[0].Title);
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
            CreatedAt = updatedAt.AddMinutes(-5),
            UpdatedAt = updatedAt
        };
    }

    private class InMemoryDashboardRepository : IDashboardRepository
    {
        private readonly List<Request> _requests;
        private readonly List<User> _users;
        private readonly int _commentsCount;
        private readonly int _auditCount;

        public InMemoryDashboardRepository(IEnumerable<Request> requests, IEnumerable<User> users, int commentsCount, int auditCount)
        {
            _requests = requests.ToList();
            _users = users.ToList();
            _commentsCount = commentsCount;
            _auditCount = auditCount;
        }

        public Task<List<Request>> GetDashboardRequestsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var requests = _requests
                .Where(r => r.CreatedByUserId == userId || r.AssignedReviewerId == userId)
                .OrderByDescending(r => r.UpdatedAt)
                .ToList();

            return Task.FromResult(requests);
        }

        public Task<int> CountRequestsAsync(Expression<Func<Request, bool>> predicate, CancellationToken cancellationToken)
        {
            return Task.FromResult(_requests.AsQueryable().Count(predicate));
        }

        public Task<List<Request>> GetRecentRequestsAsync(Expression<Func<Request, bool>> predicate, int take, CancellationToken cancellationToken)
        {
            var requests = _requests.AsQueryable()
                .Where(predicate)
                .OrderByDescending(r => r.UpdatedAt)
                .Take(take)
                .ToList();

            return Task.FromResult(requests);
        }

        public Task<int> CountUsersAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.Count);
        }

        public Task<int> CountCommentsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_commentsCount);
        }

        public Task<int> CountAuditEntriesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_auditCount);
        }
    }
}
