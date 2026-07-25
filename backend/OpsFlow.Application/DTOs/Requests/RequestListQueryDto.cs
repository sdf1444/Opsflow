using OpsFlow.Domain.Enums;

namespace OpsFlow.Application.DTOs.Requests;

public class RequestListQueryDto
{
  public int Page { get; set; } = 1;

  public int PageSize { get; set; } = 10;

  public string? Search { get; set; }

  public RequestStatus? Status { get; set; }

  public RequestCategory? Category { get; set; }

  public string? SortBy { get; set; } = "updatedAt";

  public string? SortDirection { get; set; } = "desc";

  public string? Sort { get; set; }
}