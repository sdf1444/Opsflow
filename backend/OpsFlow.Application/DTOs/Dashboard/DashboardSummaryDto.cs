namespace OpsFlow.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
  public int DraftCount { get; set; }

  public int SubmittedCount { get; set; }

  public int PendingApprovalCount { get; set; }

  public int ApprovedCount { get; set; }

  public int RejectedCount { get; set; }

  public int? TotalAssignedCount { get; set; }

  public int? TotalUsers { get; set; }

  public int? TotalRequests { get; set; }

  public int? TotalComments { get; set; }

  public int? TotalAuditEntries { get; set; }

  public List<RecentRequestDto> RecentRequests { get; set; } = new();
}
