namespace Billing.Infrastructure.PaymentGateways;

public interface IPaymentGatewayResolver
{
    /// <summary>
    /// Finds a registered payment gateway by its ID, case-insensitively.
    /// </summary>
    /// <param name="gatewayId">The gateway ID from the order.</param>
    /// <returns>The gateway, or null if no gateway has that ID.</returns>
    IPaymentGateway? Resolve(string gatewayId);

    /// <summary>
    /// Lists all registered payment gateways.
    /// </summary>
    /// <returns>The gateways, in no particular order.</returns>
    IReadOnlyList<IPaymentGateway> GetAll();
}
