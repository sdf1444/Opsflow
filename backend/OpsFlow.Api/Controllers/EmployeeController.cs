using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OpsFlow.Api.Controllers;

[ApiController]
[Route("api/employee")]
[Authorize(Policy = "EmployeeOnly")]
public class EmployeeController : ControllerBase
{
  [HttpGet("dashboard")]
  public IActionResult Dashboard()
  {
    return Ok(new
    {
      Message = "Employee dashboard"
    });
  }
}