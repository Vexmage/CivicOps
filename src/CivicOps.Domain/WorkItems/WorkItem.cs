namespace CivicOps.Domain.WorkItems;

public sealed class WorkItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public WorkItemStatus Status { get; private set; } = WorkItemStatus.Todo;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public WorkItem(string title, string? description)
    {
        Title = NormalizeTitle(title);
        Description = description;
        Touch();
    }

    public void Update(string? title, string? description, WorkItemStatus? status)
    {
        if (title is not null) Title = NormalizeTitle(title);
        if (description is not null) Description = description;
        if (status is not null) Status = status.Value;
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    private static string NormalizeTitle(string title)
    {
        title = (title ?? "").Trim();
        if (title.Length == 0) throw new ArgumentException("Title is required.");
        if (title.Length > 120) throw new ArgumentException("Title must be 120 characters or less.");
        return title;
    }
}
