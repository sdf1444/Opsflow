using Microsoft.EntityFrameworkCore;
using OpsFlow.Application.Interfaces;
using OpsFlow.Domain.Entities;
using OpsFlow.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace OpsFlow.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
  private readonly AppDbContext _dbContext;

  public DashboardRepository(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<List<Request>> GetDashboardRequestsAsync(Guid userId, CancellationToken cancellationToken)
  {
    return await _dbContext.Requests
      .Where(r => r.CreatedByUserId == userId || r.AssignedReviewerId == userId)
      .Include(r => r.CreatedByUser)
      .Include(r => r.AssignedReviewer)
      .OrderByDescending(r => r.UpdatedAt)
      .ToListAsync(cancellationToken);
  }

  public Task<int> CountRequestsAsync(Expression<Func<Request, bool>> predicate, CancellationToken cancellationToken)
  {
    return _dbContext.Requests.CountAsync(predicate, cancellationToken);
  }

  public async Task<List<Request>> GetRecentRequestsAsync(Expression<Func<Request, bool>> predicate, int take, CancellationToken cancellationToken)
  {
    return await _dbContext.Requests
      .Where(predicate)
      .Include(r => r.CreatedByUser)
      .Include(r => r.AssignedReviewer)
      .OrderByDescending(r => r.UpdatedAt)
      .Take(take)
      .ToListAsync(cancellationToken);
  }

  public Task<int> CountUsersAsync(CancellationToken cancellationToken)
  {
    return _dbContext.Users.CountAsync(cancellationToken);
  }

  public Task<int> CountCommentsAsync(CancellationToken cancellationToken)
  {
    return _dbContext.RequestComments.CountAsync(cancellationToken);
  }

  public Task<int> CountAuditEntriesAsync(CancellationToken cancellationToken)
  {
    return _dbContext.AuditLogs.CountAsync(cancellationToken);
  }
}
