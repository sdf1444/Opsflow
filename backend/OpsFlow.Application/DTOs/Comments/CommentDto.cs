namespace OpsFlow.Application.DTOs.Comments;

public class CommentDto
{
  public Guid Id { get; set; }

  public string AuthorName { get; set; } = string.Empty;

  public string AuthorEmail { get; set; } = string.Empty;

  public string Body { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; }
}
