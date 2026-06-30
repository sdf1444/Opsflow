using MediatR;
using OpsFlow.Application.Interfaces;
using OpsFlow.Domain.Entities;

namespace OpsFlow.Application.Requests.Handlers;

public class CreateRequestHandler : IRequestHandler<OpsFlow.Application.Requests.Commands.CreateRequestCommand, Request>
{
    private readonly IRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;
    private readonly OpsFlow.Application.Interfaces.IAuditService _auditService;

    public CreateRequestHandler(
        IRequestRepository requestRepository,
        IUserRepository userRepository,
        OpsFlow.Application.Interfaces.IAuditService auditService)
    {
        _requestRepository = requestRepository;
        _userRepository = userRepository;
        _auditService = auditService;
    }

    public async Task<Request> Handle(OpsFlow.Application.Requests.Commands.CreateRequestCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var entity = new Request
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            Status = Domain.Enums.RequestStatus.Draft,
            CreatedByUserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AssignedReviewerId = request.AssignedReviewerId
        };

        await _requestRepository.AddAsync(entity, cancellationToken);
        await _auditService.LogAsync(entity.Id, request.UserId, "RequestCreated", "Created draft request.", null, cancellationToken);
        await _requestRepository.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
