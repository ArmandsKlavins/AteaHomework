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

    public async Task AddAsync(Receipt receipt, CancellationToken cancellationToken)
    {
        await Task.FromResult(_receipts.TryAdd(receipt.OrderNumber, receipt));
    }

    public async Task<Receipt?> GetAsync(string orderNumber, CancellationToken cancellationToken)
    {
        return await Task.FromResult(_receipts.GetValueOrDefault(orderNumber));
    }
}
