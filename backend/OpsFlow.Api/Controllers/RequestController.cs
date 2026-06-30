using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsFlow.Application.DTOs.Requests;
using OpsFlow.Application.Services;
using MediatR;
using OpsFlow.Application.Requests.Commands;

namespace OpsFlow.Api.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize(Policy = "AuthenticatedUser")]
public class RequestController : ControllerBase
{
  private readonly IMediator _mediator;
  private readonly RequestService _requestService;

  public RequestController(IMediator mediator, RequestService requestService)
  {
    _mediator = mediator;
    _requestService = requestService;
  }

  [HttpPost]
  [Authorize(Policy = "EmployeeOnly")]
  public async Task<IActionResult> Create(CreateRequestDto requestDto, CancellationToken cancellationToken)
  {
    var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdValue, out var userId))
    {
      return Unauthorized();
    }

    var cmd = new CreateRequestCommand
    {
      UserId = userId,
      Title = requestDto.Title,
      Description = requestDto.Description,
      Category = requestDto.Category,
      AssignedReviewerId = requestDto.AssignedReviewerId
    };

    var request = await _mediator.Send(cmd, cancellationToken);
    return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
  }

  [HttpPut("{id}")]
  [Authorize(Policy = "EmployeeOnly")]
  public async Task<IActionResult> Update(Guid id, UpdateRequestDto requestDto, CancellationToken cancellationToken)
  {
    var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdValue, out var userId))
    {
      return Unauthorized();
    }

    var request = await _requestService.UpdateDraftAsync(userId, id, requestDto, cancellationToken);
    return Ok(request);
  }

  [HttpPost("{id}/submit")]
  [Authorize(Policy = "EmployeeOnly")]
  public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
  {
    var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdValue, out var userId))
    {
      return Unauthorized();
    }

    var request = await _requestService.SubmitAsync(userId, id, cancellationToken);
    return Ok(request);
  }

  [HttpPost("{id}/approve")]
  [Authorize(Policy = "ManagerOrAdmin")]
  public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
  {
    var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdValue, out var userId))
    {
      return Unauthorized();
    }

    var request = await _requestService.ApproveAsync(userId, id, cancellationToken);
    return Ok(request);
  }

  [HttpPost("{id}/reject")]
  [Authorize(Policy = "ManagerOrAdmin")]
  public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
  {
    var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdValue, out var userId))
    {
      return Unauthorized();
    }

    var request = await _requestService.RejectAsync(userId, id, cancellationToken);
    return Ok(request);
  }

  [HttpGet]
  [Authorize(Policy = "ManagerOrAdmin")]
  public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
  {
    var requests = await _requestService.GetAllAsync(cancellationToken);
    return Ok(requests);
  }

  [HttpGet("pending")]
  [Authorize(Policy = "ManagerOrAdmin")]
  public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
  {
    var requests = await _requestService.GetPendingAsync(cancellationToken);
    return Ok(requests);
  }

  [HttpPost("{id}/cancel")]
  [Authorize(Policy = "EmployeeOnly")]
  public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
  {
    var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdValue, out var userId))
    {
      return Unauthorized();
    }

    var request = await _requestService.CancelAsync(userId, id, cancellationToken);
    return Ok(request);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
  {
    var request = await _requestService.GetByIdAsync(id, cancellationToken);
    if (request is null)
    {
      return NotFound();
    }

    return Ok(request);
  }

  [HttpGet("{id}/audit")]
  public async Task<IActionResult> GetAudit(Guid id, CancellationToken cancellationToken)
  {
    var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdValue, out var userId))
    {
      return Unauthorized();
    }

    var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

    var request = await _requestService.GetByIdAsync(id, cancellationToken);
    if (request is null)
    {
      return NotFound();
    }

    // allow owner, managers, admins
    if (request.CreatedByUserId != userId && role != "Manager" && role != "Admin")
    {
      return Forbid();
    }

    var timeline = request.AuditLogs
      .OrderBy(a => a.CreatedAt)
      .Select(a => new AuditLogDto
      {
        Id = a.Id,
        RequestId = a.RequestId,
        UserId = a.UserId,
        Action = a.Action,
        Description = a.Description,
        Metadata = a.Metadata,
        CreatedAt = a.CreatedAt
      });

    return Ok(timeline);
  }
}
