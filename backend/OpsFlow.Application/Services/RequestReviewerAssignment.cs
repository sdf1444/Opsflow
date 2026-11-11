using OpsFlow.Application.Interfaces;
using OpsFlow.Domain.Enums;

namespace OpsFlow.Application.Services;

internal static class RequestReviewerAssignment
{
  public static async Task<Guid> ResolveAsync(
    Guid? assignedReviewerId,
    IUserRepository userRepository,
    IRequestRepository requestRepository,
    CancellationToken cancellationToken)
  {
    if (assignedReviewerId.HasValue)
    {
      var assignedReviewer = await userRepository.GetByIdAsync(assignedReviewerId.Value, cancellationToken);
      if (assignedReviewer?.Role == UserRole.Manager)
      {
        return assignedReviewer.Id;
      }
    }

    return await requestRepository.GetLeastLoadedReviewerIdAsync(cancellationToken)
      ?? throw new InvalidOperationException("No managers are available to review requests.");
  }
}
