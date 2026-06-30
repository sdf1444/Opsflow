using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OpsFlow.Api.Controllers;

[ApiController]
[Route("api/manager")]
[Authorize(Policy = "ManagerOnly")]
public class ManagerController : ControllerBase
{
  [HttpGet("approvals")]
  public IActionResult ApprovalQueue()
  {
    return Ok(new
    {
      Message = "Pending approvals"
    });
  }
}