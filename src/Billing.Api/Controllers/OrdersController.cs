using Billing.Contracts.Orders;
using Billing.Contracts.Receipts;
using Billing.Logic.Models;
using Billing.Logic.Services;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Processes an order by charging it through the chosen payment gateway. Each order number can only be processed once.
    /// </summary>
    /// <param name="request">The order to process.</param>
    /// <response code="200">The payment was approved; returns the receipt.</response>
    /// <response code="400">The request failed validation, or the payment gateway is unknown.</response>
    /// <response code="402">The payment gateway declined the payment.</response>
    /// <response code="409">An order with the same order number was already processed.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReceiptResponse>> ProcessOrder(OrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.ProcessOrderAsync(request, cancellationToken);

        switch (result.Status)
        {
            case OrderResultStatus.UnknownGateway:
                return Problem(statusCode: StatusCodes.Status400BadRequest,
                    detail: $"Payment gateway '{request.PaymentGatewayId}' is not supported.");
            case OrderResultStatus.Duplicate:
                return Problem(statusCode: StatusCodes.Status409Conflict,
                    detail: $"Order '{request.OrderNumber}' has already been processed.");
            case OrderResultStatus.Declined:
                return Problem(statusCode: StatusCodes.Status402PaymentRequired, title: "Payment declined",
                    detail: result.DeclineReason);
            default:
                return Ok(result.Receipt);
        }
    }
}
