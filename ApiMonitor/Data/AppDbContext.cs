using ApiMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMonitor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<ApiLog> ApiLogs => Set<ApiLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Method).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Path).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ProviderName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Host).IsRequired().HasMaxLength(300);
            entity.Property(e => e.OriginHost).IsRequired().HasMaxLength(300);

            entity.HasIndex(e => e.ReceivedAt);
            entity.HasIndex(e => e.ProviderName);
            entity.HasIndex(e => e.StatusCode);
        });
    }
}
