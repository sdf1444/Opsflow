using OpsFlow.Domain.Enums;

namespace OpsFlow.Application.DTOs.Requests;

public sealed class CreateRequestRequest
{
  public string Title { get; init; } = string.Empty;

  public string Description { get; init; } = string.Empty;

  public RequestCategory Category { get; init; }

  public bool Submit { get; init; }
}