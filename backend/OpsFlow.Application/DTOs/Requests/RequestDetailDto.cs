using OpsFlow.Domain.Enums;

namespace OpsFlow.Application.DTOs.Requests;

public class RequestDetailDto
{
  public Guid Id { get; set; }

  public string Title { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public RequestCategory Category { get; set; }

  public RequestStatus Status { get; set; }

  public UserSummaryDto CreatedBy { get; set; } = null!;

  public UserSummaryDto? AssignedReviewer { get; set; }

  public DateTime CreatedAt { get; set; }

  public DateTime UpdatedAt { get; set; }

  public DateTime? SubmittedAt { get; set; }

  public DateTime? ReviewedAt { get; set; }

  public DateTime? CancelledAt { get; set; }

  public string? RejectionReason { get; set; }

  public List<CommentDetailDto> Comments { get; set; } = new();

  public List<AuditEntryDetailDto> AuditLogs { get; set; } = new();
}
