using OpsFlow.Domain.Common;
using OpsFlow.Domain.Enums;

namespace OpsFlow.Domain.Entities;

public class Request : BaseEntity
{
  public string Title { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public RequestCategory Category { get; set; }

  public RequestStatus Status { get; set; }

  public Guid CreatedByUserId { get; set; }

  public User CreatedByUser { get; set; } = null!;

  public Guid? AssignedReviewerId { get; set; }

  public User? AssignedReviewer { get; set; }

  public DateTime? SubmittedAt { get; set; }

  public DateTime? ReviewedAt { get; set; }

  public ICollection<RequestComment> Comments { get; set; }
    = new List<RequestComment>();

  public ICollection<AuditLog> AuditLogs { get; set; }
    = new List<AuditLog>();
}