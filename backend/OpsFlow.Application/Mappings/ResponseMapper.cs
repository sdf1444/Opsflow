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
      CreatedByName = request.CreatedByUser?.Name ?? string.Empty,
      AssignedReviewerName = request.AssignedReviewer?.Name,
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

  public RequestDetailDto MapRequestDetail(Request request)
  {
    return new RequestDetailDto
    {
      Id = request.Id,
      Title = request.Title,
      Description = request.Description,
      Category = request.Category,
      Status = request.Status,
      CreatedBy = new UserSummaryDto
      {
        Id = request.CreatedByUser.Id,
        Name = request.CreatedByUser.Name,
        Email = request.CreatedByUser.Email
      },
      AssignedReviewer = request.AssignedReviewer is null ? null : new UserSummaryDto
      {
        Id = request.AssignedReviewer.Id,
        Name = request.AssignedReviewer.Name,
        Email = request.AssignedReviewer.Email
      },
      CreatedAt = request.CreatedAt,
      UpdatedAt = request.UpdatedAt,
      SubmittedAt = request.SubmittedAt,
      ReviewedAt = request.ReviewedAt,
      CancelledAt = request.CancelledAt,
      RejectionReason = request.RejectionReason,
      Comments = request.Comments
        .OrderBy(c => c.CreatedAt)
        .Select(c => new CommentDetailDto
        {
          Id = c.Id,
          Content = c.Content,
          CreatedAt = c.CreatedAt,
          Author = new UserSummaryDto
          {
            Id = c.User.Id,
            Name = c.User.Name,
            Email = c.User.Email
          }
        }).ToList(),
      AuditLogs = request.AuditLogs
        .OrderBy(a => a.CreatedAt)
        .Select(a => new AuditEntryDetailDto
        {
          Id = a.Id,
          Action = a.Action,
          Description = string.IsNullOrWhiteSpace(a.Description) ? null : a.Description,
          CreatedAt = a.CreatedAt,
          PerformedBy = new UserSummaryDto
          {
            Id = a.User.Id,
            Name = a.User.Name,
            Email = a.User.Email
          }
        }).ToList()
    };
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
      Content = comment.Content,
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
        Content = comment.Content,
        CreatedAt = comment.CreatedAt
      };
    }).ToList();
  }
}
