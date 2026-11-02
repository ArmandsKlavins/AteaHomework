using System.Collections.Concurrent;
using Billing.Domain;

namespace Billing.Infrastructure.Repositories;

/// <summary>
/// Stores receipts in memory, keyed by order number (one receipt per paid order).
/// Receipts are lost on restart, swap for a database-backed repository when persistence is needed.
/// </summary>
public class InMemoryReceiptRepository : IReceiptRepository
{
    private readonly ConcurrentDictionary<string, Receipt> _receipts = new();

    public Task AddAsync(Receipt receipt, CancellationToken cancellationToken)
    {
        _receipts.TryAdd(receipt.OrderNumber, receipt);
        return Task.CompletedTask;
    }

    public Task<Receipt?> GetAsync(string orderNumber, CancellationToken cancellationToken)
    {
        return Task.FromResult(_receipts.GetValueOrDefault(orderNumber));
    }
}
