using OpsFlow.Application.DTOs.Requests;
using OpsFlow.Application.Interfaces;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;

namespace OpsFlow.Application.Services;

public class RequestService
{
  private readonly IRequestRepository _requestRepository;
  private readonly IUserRepository _userRepository;
  private readonly OpsFlow.Application.Interfaces.IAuditService _auditService;

  public RequestService(
    IRequestRepository requestRepository,
    IUserRepository userRepository,
    OpsFlow.Application.Interfaces.IAuditService auditService)
  {
    _requestRepository = requestRepository;
    _userRepository = userRepository;
    _auditService = auditService;
  }

  public async Task<Request> CreateRequestAsync(Guid userId, CreateRequestDto requestDto, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
    if (user is null)
    {
      throw new InvalidOperationException("User not found.");
    }

    var request = new Request
    {
      Id = Guid.NewGuid(),
      Title = requestDto.Title,
      Description = requestDto.Description,
      Category = requestDto.Category,
      Status = RequestStatus.Draft,
      CreatedByUserId = userId,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow,
      AssignedReviewerId = requestDto.AssignedReviewerId
    };

    await _requestRepository.AddAsync(request, cancellationToken);
    await _auditService.LogAsync(request.Id, userId, "RequestCreated", "Created draft request.", null, cancellationToken);
    await _requestRepository.SaveChangesAsync(cancellationToken);

    return request;
  }

  public async Task<Request> UpdateDraftAsync(Guid userId, Guid requestId, UpdateRequestDto requestDto, CancellationToken cancellationToken)
  {
      var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
      if (user is null)
      {
        throw new InvalidOperationException("User not found.");
      }

      var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
      if (request is null)
      {
        throw new InvalidOperationException("Request not found.");
      }

    if (request.Status != RequestStatus.Draft)
    {
      throw new InvalidOperationException("Cannot edit a submitted request.");
    }

    if (request.CreatedByUserId != userId)
    {
      throw new UnauthorizedAccessException("You are not authorized to edit this request.");
    }

    request.Title = requestDto.Title;
    request.Description = requestDto.Description;
    request.Category = requestDto.Category;
    request.AssignedReviewerId = requestDto.AssignedReviewerId;
    request.UpdatedAt = DateTime.UtcNow;

    await _auditService.LogAsync(request.Id, userId, "RequestUpdated", "Updated request details.", null, cancellationToken);
    await _requestRepository.SaveChangesAsync(cancellationToken);
    return request;
  }

  public async Task<Request> SubmitAsync(Guid userId, Guid requestId, CancellationToken cancellationToken)
  {
    var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
    if (request is null)
    {
      throw new InvalidOperationException("Request not found.");
    }

    if (request.Status != RequestStatus.Draft)
    {
      throw new InvalidOperationException("Request must be in Draft status to submit.");
    }

    if (request.CreatedByUserId != userId)
    {
      throw new UnauthorizedAccessException("You are not authorized to submit this request.");
    }

    if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description) || request.Category == default || request.AssignedReviewerId == null)
    {
      throw new InvalidOperationException("Request must have title, description, category, and reviewer before submitting.");
    }

    request.Status = RequestStatus.Submitted;
    request.SubmittedAt = DateTime.UtcNow;
    request.UpdatedAt = DateTime.UtcNow;

    await _auditService.LogAsync(request.Id, userId, "RequestSubmitted", "Submitted request for review.", null, cancellationToken);
    await _requestRepository.SaveChangesAsync(cancellationToken);
    return request;
  }

  public async Task<Request> ApproveAsync(Guid userId, Guid requestId, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
    if (user is null)
    {
      throw new InvalidOperationException("User not found.");
    }

    if (user.Role != UserRole.Manager && user.Role != UserRole.Admin)
    {
      throw new UnauthorizedAccessException("Only managers or admins can approve requests.");
    }

    var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
    if (request is null)
    {
      throw new InvalidOperationException("Request not found.");
    }

    if (request.Status != RequestStatus.Submitted)
    {
      throw new InvalidOperationException("Only submitted requests can be approved.");
    }

    request.Status = RequestStatus.Approved;
    request.ReviewedAt = DateTime.UtcNow;
    request.UpdatedAt = DateTime.UtcNow;

    await _auditService.LogAsync(request.Id, userId, "RequestApproved", "Approved by manager.", null, cancellationToken);
    await _requestRepository.SaveChangesAsync(cancellationToken);
    return request;
  }

  public async Task<Request> RejectAsync(Guid userId, Guid requestId, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
    if (user is null)
    {
      throw new InvalidOperationException("User not found.");
    }

    if (user.Role != UserRole.Manager && user.Role != UserRole.Admin)
    {
      throw new UnauthorizedAccessException("Only managers or admins can reject requests.");
    }

    var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
    if (request is null)
    {
      throw new InvalidOperationException("Request not found.");
    }

    if (request.Status != RequestStatus.Submitted)
    {
      throw new InvalidOperationException("Only submitted requests can be rejected.");
    }

    request.Status = RequestStatus.Rejected;
    request.ReviewedAt = DateTime.UtcNow;
    request.UpdatedAt = DateTime.UtcNow;

    await _auditService.LogAsync(request.Id, userId, "RequestRejected", "Rejected by manager.", null, cancellationToken);
    await _requestRepository.SaveChangesAsync(cancellationToken);
    return request;
  }

  public async Task<Request?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken)
  {
    return await _requestRepository.GetByIdAsync(requestId, cancellationToken);
  }

  public async Task<List<Request>> GetAllAsync(CancellationToken cancellationToken)
  {
    return await _requestRepository.GetAllAsync(cancellationToken);
  }

  public async Task<List<Request>> GetPendingAsync(CancellationToken cancellationToken)
  {
    return await _requestRepository.GetPendingAsync(cancellationToken);
  }

  public async Task<Request> CancelAsync(Guid userId, Guid requestId, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
    if (user is null)
    {
      throw new InvalidOperationException("User not found.");
    }

    var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
    if (request is null)
    {
      throw new InvalidOperationException("Request not found.");
    }

    if (request.CreatedByUserId != userId)
    {
      throw new UnauthorizedAccessException("You are not authorized to cancel this request.");
    }

    if (request.Status != RequestStatus.Draft && request.Status != RequestStatus.Submitted)
    {
      throw new InvalidOperationException("Only draft or submitted requests can be cancelled.");
    }

    request.Status = RequestStatus.Cancelled;
    request.UpdatedAt = DateTime.UtcNow;

    await _auditService.LogAsync(request.Id, userId, "RequestCancelled", "Cancelled by owner.", null, cancellationToken);
    await _requestRepository.SaveChangesAsync(cancellationToken);
    return request;
  }
}
