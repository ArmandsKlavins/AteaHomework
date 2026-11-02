using Billing.Contracts.Orders;
using Billing.Infrastructure.PaymentGateways;
using Billing.Infrastructure.Repositories;
using Billing.Logic.Mappings;
using Billing.Logic.Models;

namespace Billing.Logic.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentGatewayResolver _paymentGatewayResolver;

    public OrderService(IOrderRepository orderRepository, IPaymentGatewayResolver paymentGatewayResolver)
    {
        _orderRepository = orderRepository;
        _paymentGatewayResolver = paymentGatewayResolver;
    }

    public async Task<OrderResult> ProcessOrderAsync(OrderRequest request, CancellationToken cancellationToken)
    {
        // Resolve before storing, so a mistyped gateway doesn't use up the order number
        var gateway = _paymentGatewayResolver.Resolve(request.PaymentGatewayId);
        if (gateway is null)
        {
            return new OrderResult { Status = OrderResultStatus.UnknownGateway };
        }

        var order = request.ToDomain();

        // Storing claims the order number atomically, so a concurrent duplicate is never charged
        if (!await _orderRepository.TryAddAsync(order, cancellationToken))
        {
            return new OrderResult { Status = OrderResultStatus.Duplicate };
        }

        var payment = await gateway.ChargeAsync(order, cancellationToken);
        if (!payment.IsApproved)
        {
            // Nothing was charged, so free the order number for a retry
            await _orderRepository.RemoveAsync(order, CancellationToken.None);
            return new OrderResult { Status = OrderResultStatus.Declined, DeclineReason = payment.DeclineReason };
        }

        return new OrderResult { Status = OrderResultStatus.Processed, TransactionId = payment.TransactionId };
    }
}
