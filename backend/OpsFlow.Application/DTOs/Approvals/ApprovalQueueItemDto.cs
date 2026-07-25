using OpsFlow.Domain.Enums;

namespace OpsFlow.Application.DTOs.Approvals;

public class ApprovalQueueItemDto
{
  public Guid Id { get; set; }

  public string Title { get; set; } = string.Empty;

  public RequestCategory Category { get; set; }

  public RequestStatus Status { get; set; }

  public string CreatedByName { get; set; } = string.Empty;

  public DateTime SubmittedAt { get; set; }

  public DateTime UpdatedAt { get; set; }
}
