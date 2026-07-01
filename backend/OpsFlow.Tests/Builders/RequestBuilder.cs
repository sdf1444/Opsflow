using OpsFlow.Domain.Entities;
using OpsFlow.Domain.Enums;

namespace OpsFlow.Tests.Builders;

public class RequestBuilder
{
    private readonly Request _request;

    public RequestBuilder()
    {
        var ownerId = Guid.NewGuid();
        _request = new Request
        {
            Id = Guid.NewGuid(),
            Title = "Test request",
            Description = "Test description",
            Category = RequestCategory.Other,
            Status = RequestStatus.Draft,
            CreatedByUserId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public RequestBuilder Draft()
    {
        _request.Status = RequestStatus.Draft;
        return this;
    }

    public RequestBuilder Submitted()
    {
        _request.Status = RequestStatus.Submitted;
        return this;
    }

    public RequestBuilder Approved()
    {
        _request.Status = RequestStatus.Approved;
        return this;
    }

    public RequestBuilder Rejected()
    {
        _request.Status = RequestStatus.Rejected;
        return this;
    }

    public RequestBuilder AssignedTo(Guid reviewerId)
    {
        _request.AssignedReviewerId = reviewerId;
        return this;
    }

    public RequestBuilder CreatedBy(Guid userId)
    {
        _request.CreatedByUserId = userId;
        return this;
    }

    public RequestBuilder WithTitle(string title)
    {
        _request.Title = title;
        return this;
    }

    public RequestBuilder WithCategory(RequestCategory category)
    {
        _request.Category = category;
        return this;
    }

    public Request Build() => _request;
}
