using OpsFlow.Domain.Enums;

namespace OpsFlow.Application.DTOs.Requests;

public class UpdateRequestDto
{
  public string Title { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  public RequestCategory Category { get; set; }

  public Guid? AssignedReviewerId { get; set; }
}
