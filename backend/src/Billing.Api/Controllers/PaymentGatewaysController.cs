using Billing.Contracts.PaymentGateways;
using Billing.Logic.Services;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Controllers;

[ApiController]
[Route("api/payment-gateways")]
public class PaymentGatewaysController : ControllerBase
{
    private readonly IPaymentGatewayService _paymentGatewayService;

    public PaymentGatewaysController(IPaymentGatewayService paymentGatewayService)
    {
        _paymentGatewayService = paymentGatewayService;
    }

    /// <summary>
    /// Lists the payment gateways an order can be paid through. IDs are stable; clients translate them into display names.
    /// </summary>
    /// <response code="200">The available gateways.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<PaymentGatewayResponse>> GetPaymentGateways()
    {
        return Ok(_paymentGatewayService.GetPaymentGateways());
    }
}
