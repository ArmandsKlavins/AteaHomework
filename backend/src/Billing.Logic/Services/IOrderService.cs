using Billing.Contracts.Orders;
using Billing.Logic.Models;

namespace Billing.Logic.Services;

public interface IOrderService
{
    /// <summary>
    /// Charges the order through its payment gateway, unless the gateway is unknown
    /// or an order with the same order number was already processed.
    /// </summary>
    /// <param name="request">The order to process.</param>
    /// <param name="cancellationToken">Signals that the caller no longer needs the result.</param>
    /// <returns>The outcome, with the receipt or the decline reason from the gateway.</returns>
    Task<OrderResult> ProcessOrderAsync(OrderRequest request, CancellationToken cancellationToken);
}
