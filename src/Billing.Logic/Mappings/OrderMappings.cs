using Billing.Contracts.Orders;
using Billing.Domain;

namespace Billing.Logic.Mappings;

public static class OrderMappings
{
    public static Order ToDomain(this OrderRequest request)
    {
        return new Order
        {
            OrderNumber = request.OrderNumber,
            UserId = request.UserId,
            PayableAmount = request.PayableAmount,
            PaymentGatewayId = request.PaymentGatewayId,
            Description = request.Description
        };
    }
}
