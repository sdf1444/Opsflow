using OpsFlow.Domain.Enums;

namespace OpsFlow.Application.DTOs.Requests;

public class RequestListQueryDto
{
  public int Page { get; set; } = 1;

  public int PageSize { get; set; } = 20;

  public RequestStatus? Status { get; set; }

  public RequestCategory? Category { get; set; }

  public string? Sort { get; set; } = "updatedAt_desc";
}