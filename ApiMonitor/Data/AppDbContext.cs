using ApiMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMonitor.Data;

public class AppDbContext : DbContext
{
    // Constructor — recibe la configuración de conexión desde Program.cs
    // No lo escribes tú, .NET lo inyecta automáticamente
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    // DbSet = cada uno representa una tabla en SQL Server
    // Así accedes: _context.ProviderApis.ToList()
    public DbSet<ProviderApi> ProviderApis => Set<ProviderApi>();
    public DbSet<ApiMonitorLog> ApiMonitorLogs => Set<ApiMonitorLog>();

    // OnModelCreating = aquí configuras reglas extra de las tablas
    // Es opcional pero muy útil para índices y restricciones
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProviderApi>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);
            entity.Property(e => e.Url)
                  .IsRequired()
                  .HasMaxLength(500);
        });

        modelBuilder.Entity<ApiMonitorLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Índices para búsquedas rápidas
            // Sin índice, buscar en 100k logs sería muy lento
            entity.HasIndex(e => e.CheckedAt);
            entity.HasIndex(e => e.ProviderApiId);

            // Relación: 1 ProviderApi → muchos ApiMonitorLogs
            // Cascade = si borras el proveedor, se borran sus logs
            entity.HasOne(e => e.ProviderApi)
                  .WithMany(p => p.Logs)
                  .HasForeignKey(e => e.ProviderApiId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}