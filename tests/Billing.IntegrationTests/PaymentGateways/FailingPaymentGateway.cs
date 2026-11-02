using Billing.Domain;
using Billing.Infrastructure.PaymentGateways;
using Billing.Infrastructure.PaymentGateways.Models;

namespace Billing.IntegrationTests.PaymentGateways;

/// <summary>
/// Test gateway whose charge always throws, like a gateway that times out.
/// </summary>
public class FailingPaymentGateway : IPaymentGateway
{
    public string Id { get; } = "failing";

    public Task<PaymentResult> ChargeAsync(Order order, CancellationToken cancellationToken)
    {
        throw new TimeoutException("The gateway did not respond.");
    }
}
