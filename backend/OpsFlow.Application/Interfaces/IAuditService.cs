namespace OpsFlow.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(Guid requestId, Guid userId, string action, string description, string? metadata = null, CancellationToken cancellationToken = default);
}
