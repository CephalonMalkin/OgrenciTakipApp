using Microsoft.EntityFrameworkCore;
using OgrenciTakipApp.Models;

namespace OgrenciTakipApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<DailyReading> DailyReadings => Set<DailyReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Kural: Bir öğrenci için aynı güne sadece 1 okuma kaydı düşebilir (Çift kaydı / suistimali engeller)
        modelBuilder.Entity<DailyReading>()
            .HasIndex(r => new { r.StudentId, r.ReadingDate })
            .IsUnique();
    }
}