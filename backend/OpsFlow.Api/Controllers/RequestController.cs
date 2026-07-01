using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsFlow.Application.DTOs.Requests;
using OpsFlow.Application.DTOs.Comments;
using OpsFlow.Application.Services;
using MediatR;
using OpsFlow.Application.Requests.Commands;
using OpsFlow.Application.Interfaces;

namespace OpsFlow.Api.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize(Policy = "AuthenticatedUser")]
public class RequestController : ControllerBase
{
  private readonly IMediator _mediator;
  private readonly RequestService _requestService;
  private readonly IResponseMapper _responseMapper;

  public RequestController(IMediator mediator, RequestService requestService, IResponseMapper responseMapper)
  {
    _mediator = mediator;
    _requestService = requestService;
    _responseMapper = responseMapper;
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
    var response = _responseMapper.MapRequest(request);
    return CreatedAtAction(nameof(GetById), new { id = request.Id }, response);
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
    return Ok(_responseMapper.MapRequest(request));
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
    return Ok(_responseMapper.MapRequest(request));
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
    return Ok(_responseMapper.MapRequest(request));
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
    return Ok(_responseMapper.MapRequest(request));
  }

  [HttpGet]
  [Authorize(Policy = "ManagerOrAdmin")]
  public async Task<IActionResult> GetAll([FromQuery] RequestListQueryDto query, CancellationToken cancellationToken)
  {
    var (requests, totalCount, page, pageSize) = await _requestService.GetAllAsync(query, cancellationToken);
    var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

    var response = new PagedResultDto<RequestDto>
    {
      Page = page,
      PageSize = pageSize,
      TotalCount = totalCount,
      TotalPages = totalPages,
      Items = _responseMapper.MapRequests(requests)
    };

    return Ok(response);
  }

  [HttpGet("pending")]
  [Authorize(Policy = "ManagerOrAdmin")]
  public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
  {
    var requests = await _requestService.GetPendingAsync(cancellationToken);
    return Ok(_responseMapper.MapRequests(requests));
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
    return Ok(_responseMapper.MapRequest(request));
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
  {
    var request = await _requestService.GetByIdAsync(id, cancellationToken);
    if (request is null)
    {
      return NotFound();
    }

    return Ok(_responseMapper.MapRequest(request));
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

    var timeline = _responseMapper.MapAuditLogs(request.AuditLogs.OrderBy(a => a.CreatedAt).ToList());

    return Ok(timeline);
  }

  [HttpPost("{id}/comments")]
  public async Task<IActionResult> AddComment(Guid id, CreateCommentDto dto, CancellationToken cancellationToken)
  {
    var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdValue, out var userId))
    {
      return Unauthorized();
    }

    var comment = await _requestService.AddCommentAsync(userId, id, dto, cancellationToken);
    return StatusCode(StatusCodes.Status201Created, comment);
  }

  [HttpGet("{id}/comments")]
  public async Task<IActionResult> GetComments(Guid id, CancellationToken cancellationToken)
  {
    var userIdValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (!Guid.TryParse(userIdValue, out var userId))
    {
      return Unauthorized();
    }

    var comments = await _requestService.GetCommentsAsync(userId, id, cancellationToken);
    return Ok(comments);
  }
}
