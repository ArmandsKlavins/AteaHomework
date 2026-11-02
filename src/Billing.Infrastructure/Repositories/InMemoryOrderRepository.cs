using System.Collections.Concurrent;
using Billing.Domain;

namespace Billing.Infrastructure.Repositories;

/// <summary>
/// Stores orders in memory, keyed by order number.
/// Orders are lost on restart, swap for a database-backed repository when persistence is needed.
/// </summary>
public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<string, Order> _orders = new();

    public async Task<bool> TryAddAsync(Order order, CancellationToken cancellationToken)
    {
        // TryAdd is atomic, so concurrent submits of the same order number can't both succeed
        return await Task.FromResult(_orders.TryAdd(order.OrderNumber, order));
    }
}
