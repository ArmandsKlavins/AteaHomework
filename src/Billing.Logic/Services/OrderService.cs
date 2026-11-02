using Billing.Contracts.Orders;
using Billing.Infrastructure.Repositories;
using Billing.Logic.Mappings;

namespace Billing.Logic.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<bool> ProcessOrderAsync(OrderRequest request, CancellationToken cancellationToken)
    {
        return await _orderRepository.TryAddAsync(request.ToDomain(), cancellationToken);
    }
}
