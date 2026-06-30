using Microsoft.EntityFrameworkCore;
using OpsFlow.Application.Interfaces;
using OpsFlow.Domain.Entities;
using OpsFlow.Infrastructure.Persistence;
using OpsFlow.Domain.Enums;

namespace OpsFlow.Infrastructure.Repositories;

public class RequestRepository : IRequestRepository
{
  private readonly AppDbContext _dbContext;

  public RequestRepository(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task AddAsync(Request request, CancellationToken cancellationToken)
  {
    await _dbContext.Requests.AddAsync(request, cancellationToken);
  }

  public async Task AddCommentAsync(RequestComment comment, CancellationToken cancellationToken)
  {
    await _dbContext.RequestComments.AddAsync(comment, cancellationToken);
  }

  public async Task<List<Request>> GetAllAsync(CancellationToken cancellationToken)
  {
    return await _dbContext.Requests
      .Include(r => r.CreatedByUser)
      .Include(r => r.AssignedReviewer)
      .Include(r => r.Comments)
      .Include(r => r.AuditLogs)
      .ToListAsync(cancellationToken);
  }

  public async Task<List<Request>> GetPendingAsync(CancellationToken cancellationToken)
  {
    return await _dbContext.Requests
      .Where(r => r.Status == RequestStatus.Submitted || r.Status == RequestStatus.UnderReview)
      .Include(r => r.CreatedByUser)
      .Include(r => r.AssignedReviewer)
      .Include(r => r.Comments)
      .Include(r => r.AuditLogs)
      .ToListAsync(cancellationToken);
  }

  public async Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
  {
    return await _dbContext.Requests
      .Include(r => r.CreatedByUser)
      .Include(r => r.AssignedReviewer)
      .Include(r => r.Comments)
      .Include(r => r.AuditLogs)
      .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
  }

  public async Task<List<RequestComment>> GetCommentsAsync(Guid requestId, CancellationToken cancellationToken)
  {
    return await _dbContext.RequestComments
      .Where(c => c.RequestId == requestId)
      .Include(c => c.User)
      .OrderBy(c => c.CreatedAt)
      .ToListAsync(cancellationToken);
  }

  public Task SaveChangesAsync(CancellationToken cancellationToken)
  {
    return _dbContext.SaveChangesAsync(cancellationToken);
  }
}
