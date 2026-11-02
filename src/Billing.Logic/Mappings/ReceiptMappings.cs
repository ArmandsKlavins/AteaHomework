using Billing.Contracts.Receipts;
using Billing.Domain;

namespace Billing.Logic.Mappings;

public static class ReceiptMappings
{
    public static ReceiptResponse ToResponse(this Receipt receipt)
    {
        return new ReceiptResponse
        {
            OrderNumber = receipt.OrderNumber,
            Amount = receipt.Amount,
            PaidAt = receipt.PaidAt,
            TransactionId = receipt.TransactionId
        };
    }
}
