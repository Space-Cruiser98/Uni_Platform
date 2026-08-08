namespace ComponentsOrderApi.Entities;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Student";

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Email verification
    public bool EmailConfirmed { get; set; } = false;

    public string? EmailVerificationToken { get; set; }

    public DateTime? EmailVerificationTokenExpiresAt { get; set; }

    // Password reset
    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    public ICollection<Order> OrdersAsStudent { get; set; }
        = new List<Order>();

    public ICollection<OrderStatusHistory> StatusChanges { get; set; }
        = new List<OrderStatusHistory>();
}