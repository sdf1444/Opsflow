namespace OpsFlow.Application.DTOs.Requests;

public class CommentDetailDto
{
  public Guid Id { get; set; }

  public string Content { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; }

  public UserSummaryDto Author { get; set; } = null!;
}
