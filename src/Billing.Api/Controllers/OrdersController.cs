using Billing.Contracts.Orders;
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
    /// Processes an order. Each order number can only be processed once.
    /// </summary>
    /// <param name="request">The order to process.</param>
    /// <response code="200">The order was processed.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="409">An order with the same order number was already processed.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderRequest>> ProcessOrder(OrderRequest request, CancellationToken cancellationToken)
    {
        if (!await _orderService.ProcessOrderAsync(request, cancellationToken))
            return Problem(statusCode: StatusCodes.Status409Conflict,
                detail: $"Order '{request.OrderNumber}' has already been processed.");

        return Ok(request);
    }
}
