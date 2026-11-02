namespace Billing.Contracts.PaymentGateways;

/// <summary>
/// A payment gateway an order can be paid through.
/// </summary>
public record PaymentGatewayResponse
{
    /// <summary>
    /// Stable gateway ID: send it as the order's paymentGatewayId.
    /// </summary>
    public required string Id { get; init; }
}
