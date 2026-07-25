namespace OpsFlow.Application.DTOs.Approvals;

public class ApprovalQueueResponseDto
{
  public List<ApprovalQueueItemDto> Items { get; set; } = new();

  public ApprovalQueueSummaryDto Summary { get; set; } = new();

  public int Page { get; set; }

  public int PageSize { get; set; }

  public int TotalCount { get; set; }

  public int TotalPages { get; set; }
}
