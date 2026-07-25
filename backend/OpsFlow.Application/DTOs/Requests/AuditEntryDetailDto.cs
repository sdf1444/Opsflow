namespace OpsFlow.Application.DTOs.Requests;

public class AuditEntryDetailDto
{
  public Guid Id { get; set; }

  public string Action { get; set; } = string.Empty;

  public string? Description { get; set; }

  public DateTime CreatedAt { get; set; }

  public UserSummaryDto PerformedBy { get; set; } = null!;
}
