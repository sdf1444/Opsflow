using OpsFlow.Application.DTOs.Requests;
using OpsFlow.Application.DTOs.Comments;
using OpsFlow.Application.Interfaces;
using OpsFlow.Application.Services;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using OpsFlow.Application.Mappings;

namespace OpsFlow.Tests;

public class RequestServiceTests
{
    [Fact]
    public async Task EmployeeCreatesDraft()
    {
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var service = CreateService(new[] { employee });

        var request = await service.CreateRequestAsync(employee.Id, new CreateRequestDto
        {
            Title = "New laptop",
            Description = "Need a new laptop for remote work",
            Category = RequestCategory.Equipment
        }, CancellationToken.None);

        Assert.Equal(RequestStatus.Draft, request.Status);
        Assert.Equal(employee.Id, request.CreatedByUserId);
        Assert.Equal("New laptop", request.Title);
        Assert.Single(request.AuditLogs);
        Assert.Equal("RequestCreated", request.AuditLogs.Single().Action);
    }

    [Fact]
    public async Task EmployeeEditsDraft()
    {
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Draft title",
            Description = "Draft description",
            Category = RequestCategory.Training,
            Status = RequestStatus.Draft,
            CreatedByUserId = employee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = CreateService(new[] { employee }, new[] { request });

        var updated = await service.UpdateDraftAsync(employee.Id, request.Id, new UpdateRequestDto
        {
            Title = "Updated title",
            Description = "Updated description",
            Category = RequestCategory.Equipment
        }, CancellationToken.None);

        Assert.Equal("Updated title", updated.Title);
        Assert.Equal(RequestStatus.Draft, updated.Status);
        Assert.Single(updated.AuditLogs);
        Assert.Equal("RequestUpdated", updated.AuditLogs.Last().Action);
    }

    [Fact]
    public async Task EmployeeSubmitsDraft()
    {
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var reviewer = new User { Id = Guid.NewGuid(), Name = "Manager", Email = "manager@test", Role = UserRole.Manager };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Submit title",
            Description = "Submit description",
            Category = RequestCategory.SoftwareAccess,
            Status = RequestStatus.Draft,
            CreatedByUserId = employee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AssignedReviewerId = reviewer.Id
        };

        var service = CreateService(new[] { employee, reviewer }, new[] { request });

        var submitted = await service.SubmitAsync(employee.Id, request.Id, CancellationToken.None);

        Assert.Equal(RequestStatus.Submitted, submitted.Status);
        Assert.NotNull(submitted.SubmittedAt);
        Assert.Equal("RequestSubmitted", submitted.AuditLogs.Last().Action);
    }

    [Fact]
    public async Task CannotSubmitTwice()
    {
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var reviewer = new User { Id = Guid.NewGuid(), Name = "Manager", Email = "manager@test", Role = UserRole.Manager };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Submit title",
            Description = "Submit description",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Submitted,
            CreatedByUserId = employee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AssignedReviewerId = reviewer.Id
        };

        var service = CreateService(new[] { employee, reviewer }, new[] { request });

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.SubmitAsync(employee.Id, request.Id, CancellationToken.None));
    }

    [Fact]
    public async Task CannotEditApprovedRequest()
    {
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Approved title",
            Description = "Approved description",
            Category = RequestCategory.Leave,
            Status = RequestStatus.Approved,
            CreatedByUserId = employee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = CreateService(new[] { employee }, new[] { request });

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.UpdateDraftAsync(employee.Id, request.Id, new UpdateRequestDto
            {
                Title = "Attempt",
                Description = "Attempt",
                Category = RequestCategory.Other
            }, CancellationToken.None));
    }

    [Fact]
    public async Task ManagerApprovesRequest()
    {
        var manager = new User { Id = Guid.NewGuid(), Name = "Manager", Email = "manager@test", Role = UserRole.Manager };
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Approve title",
            Description = "Approve description",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Submitted,
            CreatedByUserId = employee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AssignedReviewerId = manager.Id
        };

        var service = CreateService(new[] { manager, employee }, new[] { request });

        var approved = await service.ApproveAsync(manager.Id, request.Id, CancellationToken.None);

        Assert.Equal(RequestStatus.Approved, approved.Status);
        Assert.NotNull(approved.ReviewedAt);
        Assert.Equal("RequestApproved", approved.AuditLogs.Last().Action);
    }

    [Fact]
    public async Task ManagerRejectsRequest()
    {
        var manager = new User { Id = Guid.NewGuid(), Name = "Manager", Email = "manager@test", Role = UserRole.Manager };
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Reject title",
            Description = "Reject description",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Submitted,
            CreatedByUserId = employee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AssignedReviewerId = manager.Id
        };

        var service = CreateService(new[] { manager, employee }, new[] { request });

        var rejected = await service.RejectAsync(manager.Id, request.Id, "Budget not justified.", CancellationToken.None);

        Assert.Equal(RequestStatus.Rejected, rejected.Status);
        Assert.NotNull(rejected.ReviewedAt);
        Assert.Equal("Budget not justified.", rejected.RejectionReason);
        Assert.Equal("RequestRejected", rejected.AuditLogs.Last().Action);
    }

    [Fact]
    public async Task NonOwnerCannotSubmitRequest()
    {
        var owner = new User { Id = Guid.NewGuid(), Name = "Owner", Email = "owner@test", Role = UserRole.Employee };
        var other = new User { Id = Guid.NewGuid(), Name = "Other", Email = "other@test", Role = UserRole.Employee };
        var manager = new User { Id = Guid.NewGuid(), Name = "Manager", Email = "manager@test", Role = UserRole.Manager };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Submit title",
            Description = "Submit description",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Draft,
            CreatedByUserId = owner.Id,
            AssignedReviewerId = manager.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = CreateService(new[] { owner, other, manager }, new[] { request });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.SubmitAsync(other.Id, request.Id, CancellationToken.None));
    }

    [Fact]
    public async Task UnassignedManagerCannotApproveRequest()
    {
        var assignedManager = new User { Id = Guid.NewGuid(), Name = "Assigned", Email = "assigned@test", Role = UserRole.Manager };
        var unassignedManager = new User { Id = Guid.NewGuid(), Name = "Unassigned", Email = "unassigned@test", Role = UserRole.Manager };
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Approve title",
            Description = "Approve description",
            Category = RequestCategory.Training,
            Status = RequestStatus.Submitted,
            CreatedByUserId = employee.Id,
            AssignedReviewerId = assignedManager.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = CreateService(new[] { assignedManager, unassignedManager, employee }, new[] { request });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.ApproveAsync(unassignedManager.Id, request.Id, CancellationToken.None));
    }

    [Fact]
    public async Task ApprovedRequestCannotBeCancelled()
    {
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Cancel title",
            Description = "Cancel description",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Approved,
            CreatedByUserId = employee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = CreateService(new[] { employee }, new[] { request });

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CancelAsync(employee.Id, request.Id, CancellationToken.None));
    }

    [Fact]
    public async Task RejectedRequestCannotBeApproved()
    {
        var manager = new User { Id = Guid.NewGuid(), Name = "Manager", Email = "manager@test", Role = UserRole.Manager };
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Rejected title",
            Description = "Rejected description",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Rejected,
            CreatedByUserId = employee.Id,
            AssignedReviewerId = manager.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = CreateService(new[] { manager, employee }, new[] { request });

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.ApproveAsync(manager.Id, request.Id, CancellationToken.None));
    }

    [Fact]
    public async Task EmployeeCannotApprove()
    {
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Cannot approve title",
            Description = "Cannot approve description",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Submitted,
            CreatedByUserId = employee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = CreateService(new[] { employee }, new[] { request });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.ApproveAsync(employee.Id, request.Id, CancellationToken.None));
    }

    [Fact]
    public async Task InvalidStateTransitionRejected()
    {
        var manager = new User { Id = Guid.NewGuid(), Name = "Manager", Email = "manager@test", Role = UserRole.Manager };
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Invalid state title",
            Description = "Invalid state description",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Approved,
            CreatedByUserId = employee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = CreateService(new[] { manager, employee }, new[] { request });

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.ApproveAsync(manager.Id, request.Id, CancellationToken.None));
    }

    [Fact]
    public async Task AuditEntryCreatedOnSubmit()
    {
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var reviewer = new User { Id = Guid.NewGuid(), Name = "Manager", Email = "manager@test", Role = UserRole.Manager };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Audit title",
            Description = "Audit description",
            Category = RequestCategory.SoftwareAccess,
            Status = RequestStatus.Draft,
            CreatedByUserId = employee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AssignedReviewerId = reviewer.Id
        };
        request.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            UserId = employee.Id,
            Action = "RequestCreated",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var service = CreateService(new[] { employee, reviewer }, new[] { request });

        var submitted = await service.SubmitAsync(employee.Id, request.Id, CancellationToken.None);

        Assert.Equal(2, submitted.AuditLogs.Count);
        Assert.Contains(submitted.AuditLogs, log => log.Action == "RequestSubmitted");
    }

    [Fact]
    public async Task EmployeeCanAddCommentToOwnRequest()
    {
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Comment title",
            Description = "Comment description",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Draft,
            CreatedByUserId = employee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = CreateService(new[] { employee }, new[] { request });

        var comment = await service.AddCommentAsync(employee.Id, request.Id, new CreateCommentDto
        {
            Content = "Please review soon."
        }, CancellationToken.None);

        Assert.Equal("Please review soon.", comment.Content);
        Assert.Equal("Employee", comment.AuthorName);
        Assert.Contains(request.AuditLogs, a => a.Action == "CommentAdded");
    }

    [Fact]
    public async Task CannotAddEmptyComment()
    {
        var employee = new User { Id = Guid.NewGuid(), Name = "Employee", Email = "employee@test", Role = UserRole.Employee };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Comment title",
            Description = "Comment description",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Draft,
            CreatedByUserId = employee.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = CreateService(new[] { employee }, new[] { request });

        await Assert.ThrowsAsync<ValidationException>(async () =>
            await service.AddCommentAsync(employee.Id, request.Id, new CreateCommentDto { Content = "   " }, CancellationToken.None));
    }

    [Fact]
    public async Task EmployeeCannotCommentOnOthersRequest()
    {
        var owner = new User { Id = Guid.NewGuid(), Name = "Owner", Email = "owner@test", Role = UserRole.Employee };
        var other = new User { Id = Guid.NewGuid(), Name = "Other", Email = "other@test", Role = UserRole.Employee };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Comment title",
            Description = "Comment description",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Draft,
            CreatedByUserId = owner.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var service = CreateService(new[] { owner, other }, new[] { request });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.AddCommentAsync(other.Id, request.Id, new CreateCommentDto { Content = "Not allowed" }, CancellationToken.None));
    }

    [Fact]
    public async Task CommentsReturnedInChronologicalOrder()
    {
        var owner = new User { Id = Guid.NewGuid(), Name = "Owner", Email = "owner@test", Role = UserRole.Employee };
        var manager = new User { Id = Guid.NewGuid(), Name = "Manager", Email = "manager@test", Role = UserRole.Manager };
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Comment title",
            Description = "Comment description",
            Category = RequestCategory.Expense,
            Status = RequestStatus.Submitted,
            CreatedByUserId = owner.Id,
            AssignedReviewerId = manager.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        request.Comments.Add(new RequestComment
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            UserId = owner.Id,
            User = owner,
            Content = "First",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-10)
        });

        request.Comments.Add(new RequestComment
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            UserId = manager.Id,
            User = manager,
            Content = "Second",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var service = CreateService(new[] { owner, manager }, new[] { request });

        var comments = await service.GetCommentsAsync(owner.Id, request.Id, CancellationToken.None);

        Assert.Equal(2, comments.Count);
        Assert.Equal("First", comments[0].Content);
        Assert.Equal("Second", comments[1].Content);
    }

    private static RequestService CreateService(IEnumerable<User> users, IEnumerable<Request>? requests = null)
    {
        var userRepository = new InMemoryUserRepository(users);
        var requestRepository = new InMemoryRequestRepository(requests ?? Array.Empty<Request>());
        var auditService = new InMemoryAuditService(requestRepository);
        var mapper = new ResponseMapper();
        return new RequestService(requestRepository, userRepository, auditService, mapper);
    }

    private class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users;

        public InMemoryUserRepository(IEnumerable<User> users)
        {
            _users = users.ToList();
        }

        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.Any(u => u.Email == email));
        }

        public Task AddAsync(User user, CancellationToken cancellationToken)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Email == email));
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        }
    }

    private class InMemoryRequestRepository : IRequestRepository
    {
        private readonly List<Request> _requests;

        public InMemoryRequestRepository(IEnumerable<Request> requests)
        {
            _requests = requests.ToList();
        }

        public Task AddAsync(Request request, CancellationToken cancellationToken)
        {
            _requests.Add(request);
            return Task.CompletedTask;
        }

        public Task<(List<Request> Requests, int TotalCount)> GetAllAsync(RequestListQueryDto query, Guid currentUserId, OpsFlow.Domain.Enums.UserRole currentUserRole, CancellationToken cancellationToken)
        {
            var requestQuery = _requests.AsQueryable();

            requestQuery = currentUserRole switch
            {
                OpsFlow.Domain.Enums.UserRole.Admin => requestQuery,
                OpsFlow.Domain.Enums.UserRole.Manager => requestQuery.Where(r => r.AssignedReviewerId == currentUserId || r.CreatedByUserId == currentUserId),
                OpsFlow.Domain.Enums.UserRole.Employee => requestQuery.Where(r => r.CreatedByUserId == currentUserId),
                _ => requestQuery.Where(r => false)
            };

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();
                requestQuery = requestQuery.Where(r =>
                    r.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (query.Status.HasValue)
            {
                requestQuery = requestQuery.Where(r => r.Status == query.Status.Value);
            }

            if (query.Category.HasValue)
            {
                requestQuery = requestQuery.Where(r => r.Category == query.Category.Value);
            }

            requestQuery = (query.SortBy ?? "updatedAt").ToLowerInvariant() switch
            {
                "createdat" when string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase) => requestQuery.OrderBy(r => r.CreatedAt),
                "createdat" => requestQuery.OrderByDescending(r => r.CreatedAt),
                "title" when string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase) => requestQuery.OrderBy(r => r.Title),
                "title" => requestQuery.OrderByDescending(r => r.Title),
                "status" when string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase) => requestQuery.OrderBy(r => r.Status),
                "status" => requestQuery.OrderByDescending(r => r.Status),
                _ when string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase) => requestQuery.OrderBy(r => r.UpdatedAt),
                _ => requestQuery.OrderByDescending(r => r.UpdatedAt)
            };

            var totalCount = requestQuery.Count();
            var paged = requestQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            return Task.FromResult((paged, totalCount));
        }

        public Task<List<Request>> GetPendingAsync(CancellationToken cancellationToken)
        {
            var pending = _requests.Where(r => r.Status == OpsFlow.Domain.Enums.RequestStatus.Submitted || r.Status == OpsFlow.Domain.Enums.RequestStatus.UnderReview).ToList();
            return Task.FromResult(pending);
        }

        public Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_requests.FirstOrDefault(r => r.Id == id));
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task AddCommentAsync(RequestComment comment, CancellationToken cancellationToken)
        {
            var req = _requests.FirstOrDefault(r => r.Id == comment.RequestId);
            if (req != null)
            {
                req.Comments.Add(comment);
            }

            return Task.CompletedTask;
        }

        public Task<List<RequestComment>> GetCommentsAsync(Guid requestId, CancellationToken cancellationToken)
        {
            var comments = _requests
                .Where(r => r.Id == requestId)
                .SelectMany(r => r.Comments)
                .OrderBy(c => c.CreatedAt)
                .ToList();

            return Task.FromResult(comments);
        }

        public void AddAuditLog(Guid requestId, OpsFlow.Domain.Entities.AuditLog log)
        {
            var req = _requests.FirstOrDefault(r => r.Id == requestId);
            if (req != null)
            {
                req.AuditLogs.Add(log);
            }
        }
    }

    private class InMemoryAuditService : OpsFlow.Application.Interfaces.IAuditService
    {
        private readonly InMemoryRequestRepository _repo;

        public InMemoryAuditService(InMemoryRequestRepository repo)
        {
            _repo = repo;
        }

        public Task LogAsync(Guid requestId, Guid userId, string action, string description, string? metadata = null, CancellationToken cancellationToken = default)
        {
            var log = new OpsFlow.Domain.Entities.AuditLog
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                UserId = userId,
                Action = action,
                Description = description,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _repo.AddAuditLog(requestId, log);
            return Task.CompletedTask;
        }
    }
}
