namespace Billing.Infrastructure.PaymentGateways.Models;

public record PaymentResult
{
    public required bool IsApproved { get; init; }

    public string? TransactionId { get; init; }

    public string? DeclineReason { get; init; }
}
