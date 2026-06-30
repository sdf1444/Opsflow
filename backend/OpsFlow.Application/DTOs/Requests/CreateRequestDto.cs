using OpsFlow.Domain.Enums;

public class CreateRequestDto
{
  public string Title { get; set; } = "";

  public string Description { get; set; } = "";

  public RequestCategory Category { get; set; }

  public Guid? AssignedReviewerId { get; set; }
}