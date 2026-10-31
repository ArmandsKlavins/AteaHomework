using Billing.Domain;

namespace Billing.Logic.Services;

public interface IOrderService
{
    /// <summary>
    /// Processes the order unless one with the same order number was already processed.
    /// </summary>
    /// <param name="order">The order to process.</param>
    /// <param name="cancellationToken">Signals that the caller no longer needs the result.</param>
    /// <returns>True if the order was processed; false if it was already processed.</returns>
    Task<bool> ProcessOrderAsync(Order order, CancellationToken cancellationToken);
}
