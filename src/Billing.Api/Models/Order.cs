using System.ComponentModel.DataAnnotations;

namespace Billing.Api.Models;

public record Order
{
    [Required]
    public required string OrderNumber { get; init; }

    [Required]
    public required string UserId { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal PayableAmount { get; init; }

    [Required]
    public required string PaymentGatewayId { get; init; }

    public string? Description { get; init; }
}
