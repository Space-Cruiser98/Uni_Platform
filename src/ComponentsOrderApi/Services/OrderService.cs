using Microsoft.EntityFrameworkCore;
using ComponentsOrderApi.Data;
using ComponentsOrderApi.DTOs;
using ComponentsOrderApi.Entities;

namespace ComponentsOrderApi.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<OrderDto?> CreateOrderAsync(
        int studentId,
        CreateOrderRequest request,
        CancellationToken ct = default)
    {
        if (request.Lines == null || request.Lines.Count == 0)
            return null;

        foreach (var line in request.Lines)
        {
            if (string.IsNullOrWhiteSpace(line.ComponentName) ||
                line.Quantity <= 0)
                return null;
        }

        var order = new Order
        {
            StudentId = studentId,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var line in request.Lines)
        {
            order.Lines.Add(new OrderLine
            {
                ComponentName = line.ComponentName.Trim(),
                Quantity = line.Quantity,
                Description = line.Description?.Trim()
            });
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        return await GetOrderByIdAsync(
            order.Id,
            studentId,
            false,
            ct);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(
        int orderId,
        int? userId,
        bool isAdmin,
        CancellationToken ct = default)
    {
        var query = _db.Orders
            .Include(o => o.Student)
            .Include(o => o.ProcessedByUser)
            .Include(o => o.Lines)
            .Include(o => o.StatusHistory)
                .ThenInclude(h => h.ChangedByUser)
            .AsNoTracking();

        if (!isAdmin && userId.HasValue)
            query = query.Where(o => o.StudentId == userId);

        var order = await query
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

        return order == null ? null : MapToDto(order);
    }

    public async Task<IReadOnlyList<OrderListDto>>
        GetOrdersForStudentAsync(
            int studentId,
            CancellationToken ct = default)
    {
        return await _db.Orders
            .AsNoTracking()
            .Where(o => o.StudentId == studentId)
            .Include(o => o.Student)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListDto(
                o.Id,
                o.StudentId,
                o.Student.Name,
                o.Student.Email,
                o.Status.ToString(),
                o.CreatedAt,
                o.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OrderListDto>>
        GetOrdersForAdminAsync(
            OrderStatus? statusFilter,
            CancellationToken ct = default)
    {
        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.Student)
            .OrderByDescending(o => o.CreatedAt)
            .AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(
                o => o.Status == statusFilter.Value);

        return await query
            .Select(o => new OrderListDto(
                o.Id,
                o.StudentId,
                o.Student.Name,
                o.Student.Email,
                o.Status.ToString(),
                o.CreatedAt,
                o.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(
        int orderId,
        int adminUserId,
        UpdateOrderStatusRequest request,
        CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o => o.Student)
            .Include(o => o.Lines)
            .Include(o => o.StatusHistory)
                .ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(
                o => o.Id == orderId,
                ct);

        if (order == null)
            return null;

        var fromStatus = order.Status;
        var toStatus = request.Status;

        // SUBMITTED → APPROVED
        // SUBMITTED → REJECTED
        if (fromStatus == OrderStatus.Submitted)
        {
            if (toStatus != OrderStatus.Approved &&
                toStatus != OrderStatus.Rejected)
            {
                return null;
            }

            if (toStatus == OrderStatus.Rejected)
            {
                order.RejectionReason = request.Reason;
            }
            else
            {
                order.RejectionReason = null;
            }
        }

        // APPROVED → TAKEN
        else if (fromStatus == OrderStatus.Approved)
        {
            if (toStatus != OrderStatus.Taken)
                return null;

            order.RejectionReason = null;
        }

        // TAKEN → RETURNED
        else if (fromStatus == OrderStatus.Taken)
        {
            if (toStatus != OrderStatus.Returned)
                return null;

            order.RejectionReason = null;
        }

        // REJECTED and RETURNED are final states.
        else if (fromStatus == OrderStatus.Rejected ||
                 fromStatus == OrderStatus.Returned)
        {
            return null;
        }

        else
        {
            return null;
        }

        order.Status = toStatus;
        order.UpdatedAt = DateTime.UtcNow;
        order.ProcessedByUserId = adminUserId;

        var note =
            toStatus == OrderStatus.Rejected &&
            !string.IsNullOrWhiteSpace(request.Reason)
                ? request.Reason
                : null;

        await AddStatusHistoryAsync(
            order.Id,
            fromStatus,
            toStatus,
            adminUserId,
            note,
            ct);

        await _db.SaveChangesAsync(ct);

        return await GetOrderByIdAsync(
            order.Id,
            null,
            true,
            ct);
    }

    private async Task AddStatusHistoryAsync(
        int orderId,
        OrderStatus fromStatus,
        OrderStatus toStatus,
        int? userId,
        string? note,
        CancellationToken ct)
    {
        _db.OrderStatusHistory.Add(
            new OrderStatusHistory
            {
                OrderId = orderId,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = userId,
                Note = note
            });

        await _db.SaveChangesAsync(ct);
    }

    private static OrderDto MapToDto(Order o) => new(
        o.Id,
        o.StudentId,
        o.Student.Name,
        o.Student.Email,
        o.Status.ToString(),
        o.CreatedAt,
        o.UpdatedAt,
        o.RejectionReason,
        o.Lines
            .Select(l => new OrderLineDto(
                l.Id,
                l.ComponentName,
                l.Quantity,
                l.Description))
            .ToList(),
        o.StatusHistory
            .OrderBy(h => h.ChangedAt)
            .Select(h => new OrderStatusHistoryDto(
                h.Id,
                h.FromStatus.ToString(),
                h.ToStatus.ToString(),
                h.ChangedAt,
                h.ChangedByUser?.Name,
                h.Note))
            .ToList()
    );
}
