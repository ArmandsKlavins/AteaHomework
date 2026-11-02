using Billing.Contracts.PaymentGateways;
using Billing.Infrastructure.PaymentGateways;

namespace Billing.Logic.Mappings;

public static class PaymentGatewayMappings
{
    public static PaymentGatewayResponse ToResponse(this IPaymentGateway gateway)
    {
        return new PaymentGatewayResponse { Id = gateway.Id };
    }
}
