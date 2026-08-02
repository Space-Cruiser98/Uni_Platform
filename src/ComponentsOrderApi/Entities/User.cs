namespace ComponentsOrderApi.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Student"; // Student | Admin
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Order> OrdersAsStudent { get; set; } = new List<Order>();
    public ICollection<OrderStatusHistory> StatusChanges { get; set; } = new List<OrderStatusHistory>();
}
