using OpsFlow.Domain.Enums;

namespace OpsFlow.Application.DTOs.Dashboard;

public class RecentRequestDto
{
  public Guid Id { get; set; }

  public string Title { get; set; } = string.Empty;

  public RequestStatus Status { get; set; }

  public DateTime UpdatedAt { get; set; }

  public string CreatedBy { get; set; } = string.Empty;
}
