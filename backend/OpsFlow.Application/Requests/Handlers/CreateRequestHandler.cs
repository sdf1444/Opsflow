using MediatR;
using OpsFlow.Application.Interfaces;
using OpsFlow.Domain.Entities;

namespace OpsFlow.Application.Requests.Handlers;

public class CreateRequestHandler : IRequestHandler<OpsFlow.Application.Requests.Commands.CreateRequestCommand, Request>
{
    private readonly IRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;

    public CreateRequestHandler(
        IRequestRepository requestRepository,
        IUserRepository userRepository)
    {
        _requestRepository = requestRepository;
        _userRepository = userRepository;
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

        entity.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            RequestId = entity.Id,
            UserId = request.UserId,
            Action = "RequestCreated",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _requestRepository.AddAsync(entity, cancellationToken);
        await _requestRepository.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
