using OpsFlow.Application.DTOs.Requests;
using OpsFlow.Application.DTOs.Comments;
using OpsFlow.Application.Interfaces;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using CommentResponseDto = OpsFlow.Application.DTOs.Comments.CommentDto;

namespace OpsFlow.Application.Services;

public class RequestService
{
  private readonly IRequestRepository _requestRepository;
  private readonly IUserRepository _userRepository;
  private readonly OpsFlow.Application.Interfaces.IAuditService _auditService;
  private readonly IResponseMapper _responseMapper;

  public RequestService(
    IRequestRepository requestRepository,
    IUserRepository userRepository,
    OpsFlow.Application.Interfaces.IAuditService auditService,
    IResponseMapper responseMapper)
  {
    _requestRepository = requestRepository;
    _userRepository = userRepository;
    _auditService = auditService;
    _responseMapper = responseMapper;
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

    if (request.Status != RequestStatus.Submitted && request.Status != RequestStatus.UnderReview)
    {
      throw new InvalidOperationException("Only submitted or under review requests can be approved.");
    }

    if (user.Role == UserRole.Manager && request.AssignedReviewerId != userId)
    {
      throw new UnauthorizedAccessException("Only the assigned reviewer can approve this request.");
    }

    request.Status = RequestStatus.Approved;
    request.ReviewedAt = DateTime.UtcNow;
    request.UpdatedAt = DateTime.UtcNow;

    await _auditService.LogAsync(request.Id, userId, "RequestApproved", "Approved by manager.", null, cancellationToken);
    await _requestRepository.SaveChangesAsync(cancellationToken);
    return request;
  }

  public async Task<Request> RejectAsync(Guid userId, Guid requestId, string reason, CancellationToken cancellationToken)
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

    if (request.Status != RequestStatus.Submitted && request.Status != RequestStatus.UnderReview)
    {
      throw new InvalidOperationException("Only submitted or under review requests can be rejected.");
    }

    if (user.Role == UserRole.Manager && request.AssignedReviewerId != userId)
    {
      throw new UnauthorizedAccessException("Only the assigned reviewer can reject this request.");
    }

    var normalizedReason = reason?.Trim() ?? string.Empty;
    if (normalizedReason.Length < 5)
    {
      throw new ValidationException("Rejection reason must be at least 5 characters.");
    }

    if (normalizedReason.Length > 1000)
    {
      throw new ValidationException("Rejection reason cannot exceed 1000 characters.");
    }

    request.Status = RequestStatus.Rejected;
    request.ReviewedAt = DateTime.UtcNow;
    request.UpdatedAt = DateTime.UtcNow;
    request.RejectionReason = normalizedReason;

    await _auditService.LogAsync(request.Id, userId, "RequestRejected", "Rejected by manager.", null, cancellationToken);
    await _requestRepository.SaveChangesAsync(cancellationToken);
    return request;
  }

  public async Task<Request?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken)
  {
    return await _requestRepository.GetByIdAsync(requestId, cancellationToken);
  }

  public async Task<(List<Request> Requests, int TotalCount, int Page, int PageSize)> GetAllAsync(Guid userId, RequestListQueryDto query, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
    if (user is null)
    {
      throw new UnauthorizedAccessException("User not found.");
    }

    var normalizedQuery = NormalizeListQuery(query);
    var (requests, totalCount) = await _requestRepository.GetAllAsync(normalizedQuery, userId, user.Role, cancellationToken);
    return (requests, totalCount, normalizedQuery.Page, normalizedQuery.PageSize);
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

    if (request.CreatedByUserId != userId && user.Role != UserRole.Admin)
    {
      throw new UnauthorizedAccessException("You are not authorized to cancel this request.");
    }

    if (request.Status == RequestStatus.Approved || request.Status == RequestStatus.Rejected || request.Status == RequestStatus.Cancelled)
    {
      throw new InvalidOperationException("Approved, rejected, or already cancelled requests cannot be cancelled.");
    }

    request.Status = RequestStatus.Cancelled;
    request.CancelledAt = DateTime.UtcNow;
    request.UpdatedAt = DateTime.UtcNow;

    var cancelledBy = request.CreatedByUserId == userId ? "owner" : "admin";
    await _auditService.LogAsync(request.Id, userId, "RequestCancelled", $"Cancelled by {cancelledBy}.", null, cancellationToken);
    await _requestRepository.SaveChangesAsync(cancellationToken);
    return request;
  }

  public async Task<CommentResponseDto> AddCommentAsync(Guid userId, Guid requestId, CreateCommentDto dto, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
    if (user is null)
    {
      throw new UnauthorizedAccessException("User not found.");
    }

    var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
    if (request is null)
    {
      throw new InvalidOperationException("Request not found.");
    }

    if (!CanComment(user, request))
    {
      throw new UnauthorizedAccessException("You are not authorized to comment on this request.");
    }

    if (string.IsNullOrWhiteSpace(dto.Content))
    {
      throw new ValidationException("Comment is required.");
    }

    var normalizedContent = dto.Content.Trim();

    if (normalizedContent.Length < 2)
    {
      throw new ValidationException("Comment must be at least 2 characters.");
    }

    if (normalizedContent.Length > 1000)
    {
      throw new ValidationException("Comment cannot exceed 1000 characters.");
    }

    var now = DateTime.UtcNow;
    var comment = new RequestComment
    {
      Id = Guid.NewGuid(),
      RequestId = requestId,
      UserId = userId,
      Content = normalizedContent,
      CreatedAt = now,
      UpdatedAt = now
    };

    await _requestRepository.AddCommentAsync(comment, cancellationToken);
    await _auditService.LogAsync(requestId, userId, "CommentAdded", "Added comment to request.", null, cancellationToken);
    await _requestRepository.SaveChangesAsync(cancellationToken);

    return _responseMapper.MapComment(comment, user.Name, user.Email);
  }

  public async Task<List<CommentResponseDto>> GetCommentsAsync(Guid userId, Guid requestId, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
    if (user is null)
    {
      throw new UnauthorizedAccessException("User not found.");
    }

    var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
    if (request is null)
    {
      throw new InvalidOperationException("Request not found.");
    }

    if (!CanComment(user, request))
    {
      throw new UnauthorizedAccessException("You are not authorized to view comments for this request.");
    }

    var comments = await _requestRepository.GetCommentsAsync(requestId, cancellationToken);

    return _responseMapper.MapComments(comments.OrderBy(c => c.CreatedAt).ToList());
  }

  private static bool CanComment(User user, Request request)
  {
    return user.Role switch
    {
      UserRole.Admin => true,
      UserRole.Manager => request.AssignedReviewerId == user.Id,
      UserRole.Employee => request.CreatedByUserId == user.Id,
      _ => false
    };
  }

  private static RequestListQueryDto NormalizeListQuery(RequestListQueryDto query)
  {
    var page = query.Page <= 0 ? 1 : query.Page;
    var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
    if (pageSize > 100)
    {
      pageSize = 100;
    }

    var sortBy = string.IsNullOrWhiteSpace(query.SortBy) ? null : query.SortBy.Trim();
    var sortDirection = string.IsNullOrWhiteSpace(query.SortDirection) ? null : query.SortDirection.Trim();

    if (!string.IsNullOrWhiteSpace(query.Sort))
    {
      var legacySort = query.Sort.Trim();
      var parts = legacySort.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      if (parts.Length == 2)
      {
        sortBy = parts[0];
        sortDirection = parts[1];
      }
    }

    sortBy ??= "updatedAt";
    sortDirection ??= "desc";

    var supportedSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
      "updatedAt",
      "createdAt",
      "title",
      "status"
    };

    var supportedSortDirections = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
      "asc",
      "desc"
    };

    if (!supportedSortFields.Contains(sortBy) || !supportedSortDirections.Contains(sortDirection))
    {
      throw new ValidationException("Unsupported sort. Use sortBy of updatedAt, createdAt, title, or status and sortDirection of asc or desc.");
    }

    return new RequestListQueryDto
    {
      Page = page,
      PageSize = pageSize,
      Search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
      Status = query.Status,
      Category = query.Category,
      SortBy = sortBy,
      SortDirection = sortDirection
    };
  }
}
