namespace Billing.Domain;

public class Receipt
{
    /// <summary>
    /// The paid order's number, linking the receipt to its <see cref="Order"/>.
    /// </summary>
    public required string OrderNumber { get; init; }

    public required decimal Amount { get; init; }

    public required DateTimeOffset PaidAt { get; init; }

    public required string TransactionId { get; init; }
}
