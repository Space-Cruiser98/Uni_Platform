namespace ComponentsOrderApi.Entities;

public class Order
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Submitted;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? RejectionReason { get; set; }
    public int? ProcessedByUserId { get; set; }

    public User Student { get; set; } = null!;
    public User? ProcessedByUser { get; set; }
    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}
