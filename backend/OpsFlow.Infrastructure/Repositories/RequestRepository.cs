using Microsoft.EntityFrameworkCore;
using OpsFlow.Application.DTOs.Requests;
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

  public async Task<(List<Request> Requests, int TotalCount)> GetAllAsync(RequestListQueryDto query, CancellationToken cancellationToken)
  {
    var requestQuery = _dbContext.Requests.AsQueryable();

    if (query.Status.HasValue)
    {
      requestQuery = requestQuery.Where(r => r.Status == query.Status.Value);
    }

    if (query.Category.HasValue)
    {
      requestQuery = requestQuery.Where(r => r.Category == query.Category.Value);
    }

    requestQuery = ApplySort(requestQuery, query.Sort);

    var totalCount = await requestQuery.CountAsync(cancellationToken);

    var items = await requestQuery
      .Include(r => r.CreatedByUser)
      .Include(r => r.AssignedReviewer)
      .Include(r => r.Comments)
      .Include(r => r.AuditLogs)
      .Skip((query.Page - 1) * query.PageSize)
      .Take(query.PageSize)
      .ToListAsync(cancellationToken);

    return (items, totalCount);
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

  private static IQueryable<Request> ApplySort(IQueryable<Request> query, string? sort)
  {
    return sort?.ToLowerInvariant() switch
    {
      "updatedat_asc" => query.OrderBy(r => r.UpdatedAt),
      "updatedat_desc" => query.OrderByDescending(r => r.UpdatedAt),
      "createdat_asc" => query.OrderBy(r => r.CreatedAt),
      "createdat_desc" => query.OrderByDescending(r => r.CreatedAt),
      "title_asc" => query.OrderBy(r => r.Title),
      "title_desc" => query.OrderByDescending(r => r.Title),
      "status_asc" => query.OrderBy(r => r.Status),
      "status_desc" => query.OrderByDescending(r => r.Status),
      _ => query.OrderByDescending(r => r.UpdatedAt)
    };
  }
}
