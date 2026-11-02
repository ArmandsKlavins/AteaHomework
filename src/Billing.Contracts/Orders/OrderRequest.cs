using System.ComponentModel.DataAnnotations;
using Billing.Contracts.Validation;

namespace Billing.Contracts.Orders;

/// <summary>
/// An order to be paid through a payment gateway.
/// </summary>
public record OrderRequest
{
    /// <summary>
    /// Unique order number, up to 50 letters, digits, '-' or '_'. An order number can only be paid once; resubmitting it is rejected.
    /// </summary>
    [Required]
    [StringLength(50)]
    // Limited characters, so the order number is always safe in the receipt URL path
    [RegularExpression("^[A-Za-z0-9_-]+$", ErrorMessage = "The field OrderNumber may only contain letters, digits, '-' and '_'.")]
    public required string OrderNumber { get; init; }

    /// <summary>
    /// Identifier of the user placing the order, up to 100 characters.
    /// </summary>
    [Required]
    [StringLength(100)]
    public required string UserId { get; init; }

    /// <summary>
    /// Amount to charge. Must be at least 0.01, with at most 2 decimal places.
    /// </summary>
    [Range(0.01, double.MaxValue)]
    [MaxDecimalPlaces(2)]
    public decimal PayableAmount { get; init; }

    /// <summary>
    /// Identifier of the payment gateway that should process the payment.
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string PaymentGatewayId { get; init; }

    /// <summary>
    /// Optional free-text description of the order, up to 500 characters.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; init; }
}
