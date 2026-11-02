using Billing.Domain;

namespace Billing.Infrastructure.Repositories;

public interface IReceiptRepository
{
    /// <summary>
    /// Stores the receipt of a paid order.
    /// </summary>
    /// <param name="receipt">The receipt to store.</param>
    /// <param name="cancellationToken">Signals that the caller no longer needs the result.</param>
    Task AddAsync(Receipt receipt, CancellationToken cancellationToken);

    /// <summary>
    /// Finds the receipt of a paid order.
    /// </summary>
    /// <param name="orderNumber">The order number the receipt belongs to.</param>
    /// <param name="cancellationToken">Signals that the caller no longer needs the result.</param>
    /// <returns>The receipt, or null if the order has not been paid.</returns>
    Task<Receipt?> GetAsync(string orderNumber, CancellationToken cancellationToken);
}
