using Billing.Contracts.Receipts;
using Billing.Domain;

namespace Billing.Logic.Services;

public interface IReceiptService
{
    /// <summary>
    /// Creates and stores the receipt of a paid order, timestamped now.
    /// </summary>
    /// <param name="order">The paid order.</param>
    /// <param name="transactionId">The payment confirmation from the gateway.</param>
    /// <param name="cancellationToken">Signals that the caller no longer needs the result.</param>
    /// <returns>The stored receipt.</returns>
    Task<ReceiptResponse> CreateReceiptAsync(Order order, string transactionId, CancellationToken cancellationToken);

    /// <summary>
    /// Finds the receipt of a paid order, e.g. for a client that lost the response to its payment.
    /// </summary>
    /// <param name="orderNumber">The order number the receipt belongs to.</param>
    /// <param name="cancellationToken">Signals that the caller no longer needs the result.</param>
    /// <returns>The receipt, or null if the order has not been paid.</returns>
    Task<ReceiptResponse?> GetReceiptAsync(string orderNumber, CancellationToken cancellationToken);
}
