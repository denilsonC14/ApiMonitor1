using ApiMonitor.Data;
using ApiMonitor.Models;
using ApiMonitor.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core con SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DatabaseConecctionSGP")
    )
);


// Mapea la sección "Alerts" del appsettings a la clase AlertSettings
builder.Services.Configure<AlertSettings>(
    builder.Configuration.GetSection("Alerts")
);

builder.Services.AddHttpClient();

builder.Services.AddSingleton<AlertService>();
builder.Services.AddScoped<ApiLogService>();

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
