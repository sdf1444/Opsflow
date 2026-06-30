using OpsFlow.Application.DTOs.Requests;
using OpsFlow.Application.Interfaces;
using OpsFlow.Application.Services;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;

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

        var rejected = await service.RejectAsync(manager.Id, request.Id, CancellationToken.None);

        Assert.Equal(RequestStatus.Rejected, rejected.Status);
        Assert.NotNull(rejected.ReviewedAt);
        Assert.Equal("RequestRejected", rejected.AuditLogs.Last().Action);
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

    private static RequestService CreateService(IEnumerable<User> users, IEnumerable<Request>? requests = null)
    {
        var userRepository = new InMemoryUserRepository(users);
        var requestRepository = new InMemoryRequestRepository(requests ?? Array.Empty<Request>());
        return new RequestService(requestRepository, userRepository);
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

        public Task<List<Request>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_requests.ToList());
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
    }
}
