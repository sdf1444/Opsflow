using System.Linq.Expressions;
using OpsFlow.Domain.Entities;

namespace OpsFlow.Application.Interfaces;

public interface IDashboardRepository
{
  Task<List<Request>> GetDashboardRequestsAsync(Guid userId, CancellationToken cancellationToken);

  Task<int> CountRequestsAsync(Expression<Func<Request, bool>> predicate, CancellationToken cancellationToken);

  Task<List<Request>> GetRecentRequestsAsync(Expression<Func<Request, bool>> predicate, int take, CancellationToken cancellationToken);

  Task<int> CountUsersAsync(CancellationToken cancellationToken);

  Task<int> CountCommentsAsync(CancellationToken cancellationToken);

  Task<int> CountAuditEntriesAsync(CancellationToken cancellationToken);
}
