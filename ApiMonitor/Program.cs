using ApiMonitor.Data;
using ApiMonitor.Models;
using ApiMonitor.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core con SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
// Mapea la sección "Alerts" del appsettings a la clase AlertSettings
builder.Services.Configure<AlertSettings>(
    builder.Configuration.GetSection("Alerts")
);

// HttpClient con timeout de 15 segundos por request
builder.Services.AddHttpClient("monitor", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHttpClient(); 



// Servicios propios
// Singleton = una sola instancia durante toda la vida de la app
// Scoped    = una instancia por request HTTP
builder.Services.AddSingleton<ApiCheckerService>();
builder.Services.AddScoped<AlertService>();

// El BackgroundService — arranca automáticamente con la aplicación
builder.Services.AddHostedService<MonitorBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();