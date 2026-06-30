using Microsoft.EntityFrameworkCore;
using OpsFlow.Application.Interfaces;
using OpsFlow.Application.Services;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;
using OpsFlow.Infrastructure.Persistence;
using OpsFlow.Infrastructure.Repositories;
using OpsFlow.Infrastructure.Services;

namespace OpsFlow.Tests;

public class TransactionalConsistencyTests
{
    [Fact]
    public async Task Submit_WhenSaveFails_DoesNotPersistAuditOrStatusChange()
    {
        var databaseName = $"opsflow-transaction-test-{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var employee = new User
        {
            Id = Guid.NewGuid(),
            Name = "Employee",
            Email = "employee@test",
            PasswordHash = "hash",
            Role = UserRole.Employee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var manager = new User
        {
            Id = Guid.NewGuid(),
            Name = "Manager",
            Email = "manager@test",
            PasswordHash = "hash",
            Role = UserRole.Manager,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "New laptop",
            Description = "Need a laptop for work",
            Category = RequestCategory.Equipment,
            Status = RequestStatus.Draft,
            CreatedByUserId = employee.Id,
            AssignedReviewerId = manager.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await using (var seedContext = new AppDbContext(options))
        {
            seedContext.Users.Add(employee);
            seedContext.Users.Add(manager);
            seedContext.Requests.Add(request);
            await seedContext.SaveChangesAsync();
        }

        await using (var operationContext = new AppDbContext(options))
        {
            var requestRepository = new FailingSaveRequestRepository(new RequestRepository(operationContext));
            var userRepository = new UserRepository(operationContext);
            var auditService = new AuditService(operationContext);
            var service = new RequestService(requestRepository, userRepository, auditService);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.SubmitAsync(employee.Id, request.Id, CancellationToken.None));
        }

        await using (var verificationContext = new AppDbContext(options))
        {
            var reloaded = await verificationContext.Requests
                .Include(r => r.AuditLogs)
                .FirstAsync(r => r.Id == request.Id);

            Assert.Equal(RequestStatus.Draft, reloaded.Status);
            Assert.DoesNotContain(reloaded.AuditLogs, log => log.Action == "RequestSubmitted");
        }
    }

    private sealed class FailingSaveRequestRepository : IRequestRepository
    {
        private readonly IRequestRepository _inner;

        public FailingSaveRequestRepository(IRequestRepository inner)
        {
            _inner = inner;
        }

        public Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => _inner.GetByIdAsync(id, cancellationToken);

        public Task<List<Request>> GetAllAsync(CancellationToken cancellationToken)
            => _inner.GetAllAsync(cancellationToken);

        public Task<List<Request>> GetPendingAsync(CancellationToken cancellationToken)
            => _inner.GetPendingAsync(cancellationToken);

        public Task AddAsync(Request request, CancellationToken cancellationToken)
            => _inner.AddAsync(request, cancellationToken);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
            => throw new InvalidOperationException("Simulated persistence failure.");
    }
}
