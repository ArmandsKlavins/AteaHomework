namespace Billing.Domain;

public class Order
{
    public required string OrderNumber { get; init; }

    public required string UserId { get; init; }

    public required decimal PayableAmount { get; init; }

    public required string PaymentGatewayId { get; init; }

    public string? Description { get; init; }
}
