using Billing.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public ActionResult<Order> ProcessOrder(Order order) => Ok(order);
}
