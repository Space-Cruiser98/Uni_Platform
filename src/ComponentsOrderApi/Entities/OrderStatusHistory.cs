namespace ComponentsOrderApi.Entities;

public class OrderStatusHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public int? ChangedByUserId { get; set; }
    public string? Note { get; set; }

    public Order Order { get; set; } = null!;
    public User? ChangedByUser { get; set; }
}
