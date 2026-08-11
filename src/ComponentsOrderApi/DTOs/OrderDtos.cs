using ComponentsOrderApi.Entities;

namespace ComponentsOrderApi.DTOs;

public record CreateOrderRequest(List<OrderLineRequest> Lines);
public record OrderLineRequest(string ComponentName, int Quantity, string? Description);

public record UpdateOrderStatusRequest(
    OrderStatus Status,
    string? Reason,
    ApprovalScope? ApprovalScope = null,
    RejectionReason? RejectionReason = null,
    ReturnCondition? ReturnCondition = null,
    string? Note = null
);

public record OrderLineDto(int Id, string ComponentName, int Quantity, string? Description);
public record OrderStatusHistoryDto(
    int Id,
    string FromStatus,
    string ToStatus,
    DateTime ChangedAt,
    string? ChangedByUserName,
    string? ApprovalScope,
    string? RejectionReason,
    string? ReturnCondition,
    string? Note
);

public record OrderDto(
    int Id,
    int StudentId,
    string StudentName,
    string StudentEmail,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? RejectionReason,
    List<OrderLineDto> Lines,
    List<OrderStatusHistoryDto> StatusHistory
);

public record OrderListDto(
    int Id,
    int StudentId,
    string StudentName,
    string StudentEmail,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
