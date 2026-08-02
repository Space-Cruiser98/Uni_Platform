using Microsoft.EntityFrameworkCore;
using ComponentsOrderApi.Entities;

namespace ComponentsOrderApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.StudentId);
            e.HasIndex(o => o.Status);
            e.HasIndex(o => o.CreatedAt);
            e.HasOne(o => o.Student).WithMany(u => u.OrdersAsStudent).HasForeignKey(o => o.StudentId);
            e.HasOne(o => o.ProcessedByUser).WithMany().HasForeignKey(o => o.ProcessedByUserId);
        });

        modelBuilder.Entity<OrderLine>(e =>
        {
            e.HasOne(l => l.Order).WithMany(o => o.Lines).HasForeignKey(l => l.OrderId);
        });

        modelBuilder.Entity<OrderStatusHistory>(e =>
        {
            e.HasIndex(h => h.OrderId);
            e.HasOne(h => h.Order).WithMany(o => o.StatusHistory).HasForeignKey(h => h.OrderId);
            e.HasOne(h => h.ChangedByUser).WithMany(u => u.StatusChanges).HasForeignKey(h => h.ChangedByUserId);
        });
    }
}
