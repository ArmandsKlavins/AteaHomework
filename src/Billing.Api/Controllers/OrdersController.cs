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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Order>> ProcessOrder(Order order, CancellationToken cancellationToken)
    {
        if (!await _orderService.ProcessOrderAsync(order, cancellationToken))
            return Problem(statusCode: StatusCodes.Status409Conflict,
                detail: $"Order '{order.OrderNumber}' has already been processed.");

        return Ok(order);
    }
}
