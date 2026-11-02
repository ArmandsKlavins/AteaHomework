namespace Billing.Logic.Models;

public record OrderResult
{
    public required OrderResultStatus Status { get; init; }

    public string? TransactionId { get; init; }

    public string? DeclineReason { get; init; }
}
