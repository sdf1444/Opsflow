using OpsFlow.Application.DTOs.Comments;
using OpsFlow.Application.DTOs.Requests;
using OpsFlow.Application.Interfaces;
using OpsFlow.Domain.Entities;
using CommentResponseDto = OpsFlow.Application.DTOs.Comments.CommentDto;

namespace OpsFlow.Application.Mappings;

public class ResponseMapper : IResponseMapper
{
  public RequestDto MapRequest(Request request)
  {
    return new RequestDto
    {
      Id = request.Id,
      Title = request.Title,
      Description = request.Description,
      Category = request.Category,
      Status = request.Status,
      CreatedByUserId = request.CreatedByUserId,
      CreatedAt = request.CreatedAt,
      UpdatedAt = request.UpdatedAt,
      SubmittedAt = request.SubmittedAt,
      ReviewedAt = request.ReviewedAt,
      AssignedReviewerId = request.AssignedReviewerId
    };
  }

  public List<RequestDto> MapRequests(IEnumerable<Request> requests)
  {
    return requests.Select(MapRequest).ToList();
  }

  public AuditLogDto MapAuditLog(AuditLog auditLog)
  {
    return new AuditLogDto
    {
      Id = auditLog.Id,
      RequestId = auditLog.RequestId,
      UserId = auditLog.UserId,
      Action = auditLog.Action,
      Description = auditLog.Description,
      Metadata = auditLog.Metadata,
      CreatedAt = auditLog.CreatedAt
    };
  }

  public List<AuditLogDto> MapAuditLogs(IEnumerable<AuditLog> auditLogs)
  {
    return auditLogs.Select(MapAuditLog).ToList();
  }

  public CommentResponseDto MapComment(RequestComment comment, string authorName, string authorEmail)
  {
    return new CommentResponseDto
    {
      Id = comment.Id,
      AuthorName = authorName,
      AuthorEmail = authorEmail,
      Body = comment.Body,
      CreatedAt = comment.CreatedAt
    };
  }

  public List<CommentResponseDto> MapComments(IEnumerable<RequestComment> comments)
  {
    return comments.Select(comment =>
    {
      var author = comment.User;
      return new CommentResponseDto
      {
        Id = comment.Id,
        AuthorName = author?.Name ?? string.Empty,
        AuthorEmail = author?.Email ?? string.Empty,
        Body = comment.Body,
        CreatedAt = comment.CreatedAt
      };
    }).ToList();
  }
}
