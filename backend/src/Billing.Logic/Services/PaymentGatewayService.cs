using Billing.Contracts.PaymentGateways;
using Billing.Infrastructure.PaymentGateways;
using Billing.Logic.Mappings;

namespace Billing.Logic.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly IPaymentGatewayResolver _paymentGatewayResolver;

    public PaymentGatewayService(IPaymentGatewayResolver paymentGatewayResolver)
    {
        _paymentGatewayResolver = paymentGatewayResolver;
    }

    public IReadOnlyList<PaymentGatewayResponse> GetPaymentGateways()
    {
        return _paymentGatewayResolver.GetAll().Select(gateway => gateway.ToResponse()).ToList();
    }
}
