using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ComponentsOrderApi.DTOs;
using ComponentsOrderApi.Entities;
using ComponentsOrderApi.Services;

namespace ComponentsOrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;
    private readonly IAuthService _auth;

    public OrdersController(IOrderService orders, IAuthService auth)
    {
        _orders = orders;
        _auth = auth;
    }

    private int? UserId => int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value, out var id) ? id : null;
    private bool IsAdmin => User.IsInRole("Admin");

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderListDto>>> GetOrders([FromQuery] string? status, CancellationToken ct)
    {
        var userId = UserId;
        if (userId == null) return Unauthorized();

        if (IsAdmin)
        {
            OrderStatus? filter = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var s))
                filter = s;
            var list = await _orders.GetOrdersForAdminAsync(filter, ct);
            return Ok(list);
        }

        var studentList = await _orders.GetOrdersForStudentAsync(userId.Value, ct);
        return Ok(studentList);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id, CancellationToken ct)
    {
        var userId = UserId;
        if (userId == null) return Unauthorized();

        var order = await _orders.GetOrderByIdAsync(id, userId, IsAdmin, ct);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var userId = UserId;
        if (userId == null) return Unauthorized();

        var order = await _orders.CreateOrderAsync(userId.Value, request, ct);
        if (order == null) return BadRequest("Invalid order: at least one line with ComponentName and Quantity > 0 required.");
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var userId = UserId;
        if (userId == null) return Unauthorized();

        var order = await _orders.UpdateOrderStatusAsync(id, userId.Value, request, ct);
        if (order == null) return BadRequest("Order not found or status transition not allowed.");
        return Ok(order);
    }
}
