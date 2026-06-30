using System.Threading;
using OpsFlow.Domain.Entities;

namespace OpsFlow.Application.Interfaces;

public interface IRequestRepository
{
  Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

  Task<List<Request>> GetAllAsync(CancellationToken cancellationToken);

  Task<List<Request>> GetPendingAsync(CancellationToken cancellationToken);

  Task AddAsync(Request request, CancellationToken cancellationToken);

  Task SaveChangesAsync(CancellationToken cancellationToken);
}