using System.ComponentModel.DataAnnotations;

namespace Billing.Contracts.Orders;

/// <summary>
/// An order to be paid through a payment gateway.
/// </summary>
public record OrderRequest
{
    /// <summary>
    /// Unique order number. An order number can only be paid once; resubmitting it is rejected.
    /// </summary>
    [Required]
    public required string OrderNumber { get; init; }

    /// <summary>
    /// Identifier of the user placing the order.
    /// </summary>
    [Required]
    public required string UserId { get; init; }

    /// <summary>
    /// Amount to charge. Must be at least 0.01.
    /// </summary>
    [Range(0.01, double.MaxValue)]
    public decimal PayableAmount { get; init; }

    /// <summary>
    /// Identifier of the payment gateway that should process the payment.
    /// </summary>
    [Required]
    public required string PaymentGatewayId { get; init; }

    /// <summary>
    /// Optional free-text description of the order.
    /// </summary>
    public string? Description { get; init; }
}
