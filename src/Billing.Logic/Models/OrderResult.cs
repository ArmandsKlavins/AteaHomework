using Billing.Contracts.Receipts;

namespace Billing.Logic.Models;

public record OrderResult
{
    public required OrderResultStatus Status { get; init; }

    public ReceiptResponse? Receipt { get; init; }

    public string? DeclineReason { get; init; }
}
