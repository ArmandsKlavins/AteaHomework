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
}
