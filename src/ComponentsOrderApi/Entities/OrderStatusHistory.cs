namespace ComponentsOrderApi.Entities;

public class OrderStatusHistory
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }

    public DateTime ChangedAt { get; set; }

    public int? ChangedByUserId { get; set; }
    public User? ChangedByUser { get; set; }

    // Additional information about the status change

    public ApprovalScope? ApprovalScope { get; set; }

    public RejectionReason? RejectionReason { get; set; }

    public ReturnCondition? ReturnCondition { get; set; }

    public string? Note { get; set; }
}
