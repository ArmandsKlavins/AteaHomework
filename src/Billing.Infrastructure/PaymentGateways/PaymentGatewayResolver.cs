namespace Billing.Infrastructure.PaymentGateways;

/// <summary>
/// Looks up gateways among all registered <see cref="IPaymentGateway"/> implementations.
/// Throws on construction if two gateways share an ID.
/// </summary>
public class PaymentGatewayResolver : IPaymentGatewayResolver
{
    private readonly Dictionary<string, IPaymentGateway> _gateways;

    public PaymentGatewayResolver(IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways.ToDictionary(gateway => gateway.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IPaymentGateway? Resolve(string gatewayId)
    {
        return _gateways.GetValueOrDefault(gatewayId);
    }
}
