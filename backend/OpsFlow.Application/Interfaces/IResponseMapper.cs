using OpsFlow.Application.DTOs.Comments;
using OpsFlow.Application.DTOs.Requests;
using OpsFlow.Domain.Entities;
using CommentResponseDto = OpsFlow.Application.DTOs.Comments.CommentDto;

namespace OpsFlow.Application.Interfaces;

public interface IResponseMapper
{
  RequestDto MapRequest(Request request);

  List<RequestDto> MapRequests(IEnumerable<Request> requests);

  AuditLogDto MapAuditLog(AuditLog auditLog);

  List<AuditLogDto> MapAuditLogs(IEnumerable<AuditLog> auditLogs);

  CommentResponseDto MapComment(RequestComment comment, string authorName, string authorEmail);

  List<CommentResponseDto> MapComments(IEnumerable<RequestComment> comments);
}
