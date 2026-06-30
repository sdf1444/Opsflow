using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OpsFlow.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
  [HttpGet("dashboard")]
  public IActionResult Dashboard()
  {
    return Ok(new
    {
      Message = "Welcome Admin"
    });
  }
}