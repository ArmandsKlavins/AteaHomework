using Billing.Data.Repositories;
using Billing.Domain;

namespace Billing.Logic.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<bool> ProcessOrderAsync(Order order, CancellationToken cancellationToken)
    {
        return await _orderRepository.TryAddAsync(order, cancellationToken);
    }
}
