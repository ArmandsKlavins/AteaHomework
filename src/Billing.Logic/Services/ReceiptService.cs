using Billing.Contracts.Receipts;
using Billing.Domain;
using Billing.Infrastructure.Repositories;
using Billing.Logic.Mappings;

namespace Billing.Logic.Services;

public class ReceiptService : IReceiptService
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly TimeProvider _timeProvider;

    public ReceiptService(IReceiptRepository receiptRepository, TimeProvider timeProvider)
    {
        _receiptRepository = receiptRepository;
        _timeProvider = timeProvider;
    }

    public async Task<ReceiptResponse> CreateReceiptAsync(Order order, string transactionId, CancellationToken cancellationToken)
    {
        var receipt = new Receipt
        {
            OrderNumber = order.OrderNumber,
            Amount = order.PayableAmount,
            PaidAt = _timeProvider.GetUtcNow(),
            TransactionId = transactionId
        };

        await _receiptRepository.AddAsync(receipt, cancellationToken);

        return receipt.ToResponse();
    }

    public async Task<ReceiptResponse?> GetReceiptAsync(string orderNumber, CancellationToken cancellationToken)
    {
        var receipt = await _receiptRepository.GetAsync(orderNumber, cancellationToken);
        return receipt?.ToResponse();
    }
}
