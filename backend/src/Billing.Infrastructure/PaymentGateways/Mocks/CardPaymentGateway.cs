using Billing.Domain;
using Billing.Infrastructure.PaymentGateways.Models;

namespace Billing.Infrastructure.PaymentGateways.Mocks;

/// <summary>
/// Mock card gateway that approves every payment.
/// </summary>
public class CardPaymentGateway : IPaymentGateway
{
    public string Id { get; } = "card";

    public Task<PaymentResult> ChargeAsync(Order order, CancellationToken cancellationToken)
    {
        return Task.FromResult(new PaymentResult { IsApproved = true, TransactionId = $"card_{Guid.NewGuid():N}" });
    }
}
