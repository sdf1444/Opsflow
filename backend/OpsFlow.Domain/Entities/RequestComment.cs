using OpsFlow.Domain.Common;

namespace OpsFlow.Domain.Entities;

public class RequestComment : BaseEntity
{
  public Guid RequestId { get; set; }

  public Request Request { get; set; } = null!;

  public Guid UserId { get; set; }

  public User User { get; set; } = null!;

  public string Body { get; set; } = string.Empty;
}