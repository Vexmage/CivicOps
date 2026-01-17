using CivicOps.Domain.WorkItems;
using System.Collections.Concurrent;

namespace CivicOps.Api.WorkItems;

public interface IWorkItemRepository
{
    IReadOnlyCollection<WorkItem> List();
    WorkItem? Get(Guid id);
    WorkItem Add(WorkItem item);
    bool TryUpdate(Guid id, Action<WorkItem> update);
    bool Delete(Guid id);
}

public sealed class InMemoryWorkItemRepository : IWorkItemRepository
{
    private readonly ConcurrentDictionary<Guid, WorkItem> _items = new();

    public IReadOnlyCollection<WorkItem> List() => _items.Values
        .OrderByDescending(x => x.UpdatedAt)
        .ToList();

    public WorkItem? Get(Guid id) => _items.TryGetValue(id, out var item) ? item : null;

    public WorkItem Add(WorkItem item)
    {
        _items[item.Id] = item;
        return item;
    }

    public bool TryUpdate(Guid id, Action<WorkItem> update)
    {
        if (!_items.TryGetValue(id, out var item)) return false;
        update(item);
        return true;
    }

    public bool Delete(Guid id) => _items.TryRemove(id, out _);
}