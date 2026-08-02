using ComponentsOrderApi.DTOs;
using ComponentsOrderApi.Entities;

namespace ComponentsOrderApi.Services;

public interface IOrderService
{
    Task<OrderDto?> CreateOrderAsync(int studentId, CreateOrderRequest request, CancellationToken ct = default);
    Task<OrderDto?> GetOrderByIdAsync(int orderId, int? userId, bool isAdmin, CancellationToken ct = default);
    Task<IReadOnlyList<OrderListDto>> GetOrdersForStudentAsync(int studentId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderListDto>> GetOrdersForAdminAsync(OrderStatus? statusFilter, CancellationToken ct = default);
    Task<OrderDto?> UpdateOrderStatusAsync(int orderId, int adminUserId, UpdateOrderStatusRequest request, CancellationToken ct = default);
}
