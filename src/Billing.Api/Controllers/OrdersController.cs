using System.Diagnostics;
using Billing.Contracts.Orders;
using Billing.Contracts.Receipts;
using Billing.Logic.Models;
using Billing.Logic.Services;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IReceiptService _receiptService;

    public OrdersController(IOrderService orderService, IReceiptService receiptService)
    {
        _orderService = orderService;
        _receiptService = receiptService;
    }

    /// <summary>
    /// Processes an order by charging it through the chosen payment gateway. Each order number can only be processed once.
    /// </summary>
    /// <param name="request">The order to process.</param>
    /// <response code="201">The payment was approved; returns the receipt, and its URL in the Location header.</response>
    /// <response code="400">The request failed validation, or the payment gateway is unknown.</response>
    /// <response code="402">The payment gateway declined the payment.</response>
    /// <response code="409">An order with the same order number was already processed; its receipt can be fetched.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
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
            case OrderResultStatus.Processed:
                return CreatedAtAction(nameof(GetReceipt), new { orderNumber = request.OrderNumber }, result.Receipt);
            default:
                throw new UnreachableException($"Unhandled order result status {result.Status}.");
        }
    }

    /// <summary>
    /// Gets the receipt of a paid order.
    /// </summary>
    /// <param name="orderNumber">The order number the receipt belongs to.</param>
    /// <response code="200">The order was paid; returns its receipt.</response>
    /// <response code="404">No paid order has this order number.</response>
    [HttpGet("{orderNumber}/receipt")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReceiptResponse>> GetReceipt(string orderNumber, CancellationToken cancellationToken)
    {
        var receipt = await _receiptService.GetReceiptAsync(orderNumber, cancellationToken);
        if (receipt is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound,
                detail: $"No receipt found for order '{orderNumber}'.");
        }

        return Ok(receipt);
    }
}
