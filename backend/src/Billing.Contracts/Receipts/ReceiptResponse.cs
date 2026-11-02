namespace Billing.Contracts.Receipts;

/// <summary>
/// Proof that an order was paid.
/// </summary>
public record ReceiptResponse
{
    /// <summary>
    /// Number of the paid order.
    /// </summary>
    public required string OrderNumber { get; init; }

    /// <summary>
    /// Amount that was charged.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// When the payment was approved, in UTC.
    /// </summary>
    public required DateTimeOffset PaidAt { get; init; }

    /// <summary>
    /// Payment confirmation: the transaction ID issued by the payment gateway.
    /// </summary>
    public required string TransactionId { get; init; }
}
