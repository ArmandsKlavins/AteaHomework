using Billing.Domain;
using Billing.Infrastructure.PaymentGateways.Models;

namespace Billing.Infrastructure.PaymentGateways.Mocks;

/// <summary>
/// Mock bank gateway that declines amounts over 1000.
/// </summary>
public class BankPaymentGateway : IPaymentGateway
{
    private const decimal MaxAmount = 1000m;

    public string Id { get; } = "bank";

    public async Task<PaymentResult> ChargeAsync(Order order, CancellationToken cancellationToken)
    {
        if (order.PayableAmount > MaxAmount)
        {
            return await Task.FromResult(new PaymentResult { IsApproved = false, DeclineReason = $"Bank payments are limited to {MaxAmount}." });
        }

        return await Task.FromResult(new PaymentResult { IsApproved = true, TransactionId = $"bank_{Guid.NewGuid():N}" });
    }
}
