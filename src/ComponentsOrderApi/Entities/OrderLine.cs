namespace ComponentsOrderApi.Entities;

public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Description { get; set; }

    public Order Order { get; set; } = null!;
}
