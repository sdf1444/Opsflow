using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsFlow.Application.Services;

namespace OpsFlow.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "AuthenticatedUser")]
public class DashboardController : ControllerBase
{
  private readonly DashboardService _dashboardService;

  public DashboardController(DashboardService dashboardService)
  {
    _dashboardService = dashboardService;
  }

  [HttpGet]
  public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
  {
    var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdValue, out var userId))
    {
      return Unauthorized();
    }

    var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

    return role switch
    {
      "Employee" => Ok(await _dashboardService.GetEmployeeDashboardAsync(userId, cancellationToken)),
      "Manager" => Ok(await _dashboardService.GetManagerDashboardAsync(userId, cancellationToken)),
      "Admin" => Ok(await _dashboardService.GetAdminDashboardAsync(cancellationToken)),
      _ => Forbid()
    };
  }
}
