namespace Billing.Infrastructure.PaymentGateways;

/// <summary>
/// Looks up gateways among all registered <see cref="IPaymentGateway"/> implementations.
/// Throws on construction if two gateways share an ID.
/// </summary>
public class PaymentGatewayResolver : IPaymentGatewayResolver
{
    private readonly Dictionary<string, IPaymentGateway> _gatewaysById;

    public PaymentGatewayResolver(IEnumerable<IPaymentGateway> gateways)
    {
        _gatewaysById = gateways.ToDictionary(gateway => gateway.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IPaymentGateway? Resolve(string gatewayId)
    {
        return _gatewaysById.GetValueOrDefault(gatewayId);
    }

    public IReadOnlyList<IPaymentGateway> GetAll()
    {
        return _gatewaysById.Values.ToList();
    }
}
