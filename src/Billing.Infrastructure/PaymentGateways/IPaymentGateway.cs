using Billing.Domain;
using Billing.Infrastructure.PaymentGateways.Models;

namespace Billing.Infrastructure.PaymentGateways;

public interface IPaymentGateway
{
    /// <summary>
    /// Identifier clients use to pick this gateway, matched case-insensitively.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Charges the order's payable amount.
    /// </summary>
    /// <param name="order">The order to charge.</param>
    /// <param name="cancellationToken">Signals that the caller no longer needs the result.</param>
    /// <returns>The transaction ID if the payment was approved, or the reason it was declined.</returns>
    Task<PaymentResult> ChargeAsync(Order order, CancellationToken cancellationToken);
}
