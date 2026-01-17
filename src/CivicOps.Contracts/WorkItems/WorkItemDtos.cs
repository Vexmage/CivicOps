using System.Text.Json.Serialization;

namespace CivicOps.Contracts.WorkItems;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkItemStatus
{
    Todo = 0,
    InProgress = 1,
    Blocked = 2,
    Done = 3
}

public sealed record WorkItemResponse(
    Guid Id,
    string Title,
    string? Description,
    WorkItemStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public sealed record CreateWorkItemRequest(
    string Title,
    string? Description
);

public sealed record UpdateWorkItemRequest(
    string? Title,
    string? Description,
    WorkItemStatus? Status
);