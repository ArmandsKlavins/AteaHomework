using Billing.Domain;

namespace Billing.Infrastructure.Repositories;

public interface IOrderRepository
{
    /// <summary>
    /// Stores the order unless one with the same order number exists.
    /// </summary>
    /// <param name="order">The order to store.</param>
    /// <param name="cancellationToken">Signals that the caller no longer needs the result.</param>
    /// <returns>True if the order was stored; false if the order number already exists.</returns>
    Task<bool> TryAddAsync(Order order, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the order, freeing its order number for a new attempt.
    /// </summary>
    /// <param name="order">The order to remove.</param>
    /// <param name="cancellationToken">Signals that the caller no longer needs the result.</param>
    Task RemoveAsync(Order order, CancellationToken cancellationToken);
}
