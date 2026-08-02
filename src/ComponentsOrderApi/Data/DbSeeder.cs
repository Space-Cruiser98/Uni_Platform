using Microsoft.EntityFrameworkCore;
using ComponentsOrderApi.Entities;

namespace ComponentsOrderApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (await db.Users.AnyAsync(u => u.Role == "Admin"))
            return;

        var admins = new[]
        {
            new User
            {
                Email = "admin1@school.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1!"),
                Role = "Admin",
                Name = "Admin 1",
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Email = "technician@school.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Technician1!"),
                Role = "Admin",
                Name = "Technician",
                CreatedAt = DateTime.UtcNow
            }
        };

        db.Users.AddRange(admins);
        await db.SaveChangesAsync();
    }
}
