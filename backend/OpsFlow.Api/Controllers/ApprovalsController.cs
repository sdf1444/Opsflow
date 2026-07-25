using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsFlow.Application.DTOs.Requests;
using OpsFlow.Application.Services;

namespace OpsFlow.Api.Controllers;

[ApiController]
[Route("api/approvals")]
[Authorize(Policy = "ManagerOrAdmin")]
public class ApprovalsController : ControllerBase
{
  private readonly RequestService _requestService;

  public ApprovalsController(RequestService requestService)
  {
    _requestService = requestService;
  }

  [HttpGet]
  public async Task<IActionResult> GetQueue([FromQuery] RequestListQueryDto query, CancellationToken cancellationToken)
  {
    var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdValue, out var userId))
    {
      return Unauthorized();
    }

    var result = await _requestService.GetApprovalQueueAsync(userId, query, cancellationToken);
    return Ok(result);
  }
}
