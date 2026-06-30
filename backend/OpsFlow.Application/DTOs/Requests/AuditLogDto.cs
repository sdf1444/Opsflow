namespace OpsFlow.Application.DTOs.Requests;

public class AuditLogDto
{
  public Guid Id { get; set; }

  public Guid RequestId { get; set; }

  public Guid UserId { get; set; }

  public string Action { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public string? Metadata { get; set; }

  public DateTime CreatedAt { get; set; }
}
