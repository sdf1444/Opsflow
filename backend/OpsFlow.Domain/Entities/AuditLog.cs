using OpsFlow.Domain.Common;

namespace OpsFlow.Domain.Entities;

public class AuditLog : BaseEntity
{
  public Guid RequestId { get; set; }

  public Request Request { get; set; } = null!;

  public Guid UserId { get; set; }

  public User User { get; set; } = null!;

  public string Action { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;

  public string? Metadata { get; set; }
}