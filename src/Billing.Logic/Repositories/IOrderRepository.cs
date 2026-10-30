using Billing.Logic.Models;

namespace Billing.Logic.Repositories;

public interface IOrderRepository
{
    /// <summary>
    /// Stores the order unless one with the same order number exists.
    /// </summary>
    /// <param name="order">The order to store.</param>
    /// <param name="cancellationToken">Signals that the caller no longer needs the result.</param>
    /// <returns>True if the order was stored; false if the order number already exists.</returns>
    Task<bool> TryAddAsync(Order order, CancellationToken cancellationToken);
}
