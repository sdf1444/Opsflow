using MediatR;
using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;

namespace OpsFlow.Application.Requests.Commands;

public class CreateRequestCommand : IRequest<Request>
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public RequestCategory Category { get; set; }

    public Guid? AssignedReviewerId { get; set; }
}
