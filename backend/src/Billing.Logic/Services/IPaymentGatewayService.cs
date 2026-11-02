using Billing.Contracts.PaymentGateways;

namespace Billing.Logic.Services;

public interface IPaymentGatewayService
{
    /// <summary>
    /// Lists the payment gateways an order can be paid through.
    /// </summary>
    /// <returns>The payment gateways.</returns>
    IReadOnlyList<PaymentGatewayResponse> GetPaymentGateways();
}
