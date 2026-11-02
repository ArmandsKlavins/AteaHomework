using Billing.Contracts.Orders;
using Billing.Infrastructure.PaymentGateways;
using Billing.Infrastructure.PaymentGateways.Models;
using Billing.Infrastructure.Repositories;
using Billing.Logic.Mappings;
using Billing.Logic.Models;
using Microsoft.Extensions.Logging;

namespace Billing.Logic.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IReceiptService _receiptService;
    private readonly IPaymentGatewayResolver _paymentGatewayResolver;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, IReceiptService receiptService, IPaymentGatewayResolver paymentGatewayResolver,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _receiptService = receiptService;
        _paymentGatewayResolver = paymentGatewayResolver;
        _logger = logger;
    }

    public async Task<OrderResult> ProcessOrderAsync(OrderRequest request, CancellationToken cancellationToken)
    {
        // Resolve before storing, so a mistyped gateway doesn't use up the order number
        var gateway = _paymentGatewayResolver.Resolve(request.PaymentGatewayId);
        if (gateway is null)
        {
            _logger.LogWarning("Order {OrderNumber} rejected: unknown payment gateway {PaymentGatewayId}",
                request.OrderNumber, request.PaymentGatewayId);
            return new OrderResult { Status = OrderResultStatus.UnknownGateway };
        }

        var order = request.ToDomain();

        // Storing claims the order number atomically, so a concurrent duplicate is never charged
        if (!await _orderRepository.TryAddAsync(order, cancellationToken))
        {
            _logger.LogWarning("Order {OrderNumber} rejected: it has already been processed", order.OrderNumber);
            return new OrderResult { Status = OrderResultStatus.Duplicate };
        }

        _logger.LogInformation("Charging order {OrderNumber} of {Amount} through gateway {PaymentGatewayId}",
            order.OrderNumber, order.PayableAmount, gateway.Id);

        PaymentResult payment;
        try
        {
            // Once started, the charge runs to the end even if the client has gone, so its outcome is known
            payment = await gateway.ChargeAsync(order, CancellationToken.None);
        }
        catch (Exception exception)
        {
            // The charge may have gone through, so the order stays stored (a retry gets 409) until someone checks it with the gateway
            _logger.LogError(exception, "Charging order {OrderNumber} through gateway {PaymentGatewayId} failed; the payment outcome is unknown",
                order.OrderNumber, gateway.Id);
            throw;
        }

        if (!payment.IsApproved)
        {
            // Nothing was charged, so free the order number for a retry
            await _orderRepository.RemoveAsync(order, CancellationToken.None);
            _logger.LogInformation("Order {OrderNumber} declined by gateway {PaymentGatewayId}: {DeclineReason}",
                order.OrderNumber, gateway.Id, payment.DeclineReason);
            return new OrderResult { Status = OrderResultStatus.Declined, DeclineReason = payment.DeclineReason };
        }

        // The payment went through, so the receipt must be stored even if the client has gone
        var receipt = await _receiptService.CreateReceiptAsync(order, payment.TransactionId!, CancellationToken.None);
        _logger.LogInformation("Order {OrderNumber} paid through gateway {PaymentGatewayId}, transaction {TransactionId}",
            order.OrderNumber, gateway.Id, payment.TransactionId);

        return new OrderResult { Status = OrderResultStatus.Processed, Receipt = receipt };
    }
}
