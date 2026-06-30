using OpsFlow.Application.Interfaces;
using OpsFlow.Infrastructure.Persistence;
using OpsFlow.Domain.Entities;

namespace OpsFlow.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(Guid requestId, Guid userId, string action, string description, string? metadata = null, CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
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

        await _db.AuditLogs.AddAsync(log, cancellationToken);
    }
}
