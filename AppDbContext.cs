using Microsoft.EntityFrameworkCore;
using StudyHub.API.Models;

namespace StudyHub.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Material)
            .WithMany(m => m.Orders)
            .HasForeignKey(o => o.MaterialId);

        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.UserId, o.MaterialId })
            .IsUnique();

        // Seed admin user (password: admin123)
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@studyhub.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin",
            CreatedAt = new DateTime(2024, 1, 1)
        });

        // Seed sample materials
        modelBuilder.Entity<Material>().HasData(
            new Material { Id = 1, Title = "Calculus Made Easy", Description = "Complete calculus notes from basics to advanced", Category = "Mathematics", Price = 0, FileUrl = "https://example.com/calculus.pdf", CreatedAt = new DateTime(2024, 1, 1) },
            new Material { Id = 2, Title = "Python for Beginners", Description = "Step-by-step Python programming guide", Category = "Programming", Price = 9.99m, FileUrl = "https://example.com/python.pdf", CreatedAt = new DateTime(2024, 1, 2) },
            new Material { Id = 3, Title = "World History Notes", Description = "Comprehensive world history summary", Category = "History", Price = 4.99m, FileUrl = "https://example.com/history.pdf", CreatedAt = new DateTime(2024, 1, 3) },
            new Material { Id = 4, Title = "Organic Chemistry", Description = "Detailed organic chemistry course notes", Category = "Chemistry", Price = 14.99m, FileUrl = "https://example.com/chem.pdf", CreatedAt = new DateTime(2024, 1, 4) },
            new Material { Id = 5, Title = "Data Structures & Algorithms", Description = "Essential DSA concepts with examples", Category = "Programming", Price = 12.99m, FileUrl = "https://example.com/dsa.pdf", CreatedAt = new DateTime(2024, 1, 5) },
            new Material { Id = 6, Title = "English Grammar Guide", Description = "Complete English grammar reference", Category = "Languages", Price = 0, FileUrl = "https://example.com/english.pdf", CreatedAt = new DateTime(2024, 1, 6) }
        );
    }
}
