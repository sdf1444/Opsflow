using OpsFlow.Application.DTOs.Dashboard;
using OpsFlow.Application.Interfaces;
using OpsFlow.Domain.Enums;

namespace OpsFlow.Application.Services;

public class DashboardService
{
  private readonly IDashboardRepository _dashboardRepository;

  public DashboardService(IDashboardRepository dashboardRepository)
  {
    _dashboardRepository = dashboardRepository;
  }

  public async Task<DashboardSummaryDto> GetEmployeeDashboardAsync(Guid userId, CancellationToken cancellationToken)
  {
    var draftCount = await _dashboardRepository.CountRequestsAsync(
      r => r.CreatedByUserId == userId && r.Status == RequestStatus.Draft,
      cancellationToken);

    var submittedCount = await _dashboardRepository.CountRequestsAsync(
      r => r.CreatedByUserId == userId && r.Status == RequestStatus.Submitted,
      cancellationToken);

    var approvedCount = await _dashboardRepository.CountRequestsAsync(
      r => r.CreatedByUserId == userId && r.Status == RequestStatus.Approved,
      cancellationToken);

    var rejectedCount = await _dashboardRepository.CountRequestsAsync(
      r => r.CreatedByUserId == userId && r.Status == RequestStatus.Rejected,
      cancellationToken);

    var pendingApprovalCount = await _dashboardRepository.CountRequestsAsync(
      r => r.CreatedByUserId == userId && (r.Status == RequestStatus.Submitted || r.Status == RequestStatus.UnderReview),
      cancellationToken);

    var recent = await _dashboardRepository.GetRecentRequestsAsync(
      r => r.CreatedByUserId == userId,
      10,
      cancellationToken);

    return new DashboardSummaryDto
    {
      DraftCount = draftCount,
      SubmittedCount = submittedCount,
      PendingApprovalCount = pendingApprovalCount,
      ApprovedCount = approvedCount,
      RejectedCount = rejectedCount,
      RecentRequests = recent.Select(MapRecent).ToList()
    };
  }

  public async Task<DashboardSummaryDto> GetManagerDashboardAsync(Guid userId, CancellationToken cancellationToken)
  {
    var submittedCount = await _dashboardRepository.CountRequestsAsync(
      r => r.AssignedReviewerId == userId && r.Status == RequestStatus.Submitted,
      cancellationToken);

    var approvedCount = await _dashboardRepository.CountRequestsAsync(
      r => r.AssignedReviewerId == userId && r.Status == RequestStatus.Approved,
      cancellationToken);

    var rejectedCount = await _dashboardRepository.CountRequestsAsync(
      r => r.AssignedReviewerId == userId && r.Status == RequestStatus.Rejected,
      cancellationToken);

    var pendingApprovalCount = await _dashboardRepository.CountRequestsAsync(
      r => r.AssignedReviewerId == userId && (r.Status == RequestStatus.Submitted || r.Status == RequestStatus.UnderReview),
      cancellationToken);

    var totalAssigned = await _dashboardRepository.CountRequestsAsync(
      r => r.AssignedReviewerId == userId,
      cancellationToken);

    var recent = await _dashboardRepository.GetRecentRequestsAsync(
      r => r.AssignedReviewerId == userId,
      10,
      cancellationToken);

    return new DashboardSummaryDto
    {
      DraftCount = 0,
      SubmittedCount = submittedCount,
      PendingApprovalCount = pendingApprovalCount,
      ApprovedCount = approvedCount,
      RejectedCount = rejectedCount,
      TotalAssignedCount = totalAssigned,
      RecentRequests = recent.Select(MapRecent).ToList()
    };
  }

  public async Task<DashboardSummaryDto> GetAdminDashboardAsync(CancellationToken cancellationToken)
  {
    var pendingApprovalCount = await _dashboardRepository.CountRequestsAsync(
      r => r.Status == RequestStatus.Submitted || r.Status == RequestStatus.UnderReview,
      cancellationToken);

    var approvedCount = await _dashboardRepository.CountRequestsAsync(
      r => r.Status == RequestStatus.Approved,
      cancellationToken);

    var rejectedCount = await _dashboardRepository.CountRequestsAsync(
      r => r.Status == RequestStatus.Rejected,
      cancellationToken);

    var submittedCount = await _dashboardRepository.CountRequestsAsync(
      r => r.Status == RequestStatus.Submitted,
      cancellationToken);

    var draftCount = await _dashboardRepository.CountRequestsAsync(
      r => r.Status == RequestStatus.Draft,
      cancellationToken);

    var totalUsers = await _dashboardRepository.CountUsersAsync(cancellationToken);
    var totalRequests = await _dashboardRepository.CountRequestsAsync(_ => true, cancellationToken);
    var totalComments = await _dashboardRepository.CountCommentsAsync(cancellationToken);
    var totalAuditEntries = await _dashboardRepository.CountAuditEntriesAsync(cancellationToken);

    var recent = await _dashboardRepository.GetRecentRequestsAsync(_ => true, 10, cancellationToken);

    return new DashboardSummaryDto
    {
      DraftCount = draftCount,
      SubmittedCount = submittedCount,
      PendingApprovalCount = pendingApprovalCount,
      ApprovedCount = approvedCount,
      RejectedCount = rejectedCount,
      TotalUsers = totalUsers,
      TotalRequests = totalRequests,
      TotalComments = totalComments,
      TotalAuditEntries = totalAuditEntries,
      RecentRequests = recent.Select(MapRecent).ToList()
    };
  }

  private static RecentRequestDto MapRecent(Domain.Entities.Request request)
  {
    return new RecentRequestDto
    {
      Id = request.Id,
      Title = request.Title,
      Status = request.Status,
      UpdatedAt = request.UpdatedAt,
      CreatedBy = request.CreatedByUser?.Name ?? string.Empty
    };
  }
}
