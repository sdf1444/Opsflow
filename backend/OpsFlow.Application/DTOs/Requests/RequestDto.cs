using OpsFlow.Domain.Enums;

namespace OpsFlow.Application.DTOs.Requests;

public class RequestDto
{
  public Guid Id { get; set; }

  public string Title { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public RequestCategory Category { get; set; }

  public RequestStatus Status { get; set; }

  public Guid CreatedByUserId { get; set; }

  public DateTime CreatedAt { get; set; }

  public DateTime UpdatedAt { get; set; }

  public DateTime? SubmittedAt { get; set; }

  public DateTime? ReviewedAt { get; set; }

  public Guid? AssignedReviewerId { get; set; }
}
