using DdzServer.Data;
using DdzServer.Models;
using DdzServer.Services;
using DdzServer.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 配置 MySQL 数据库
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))));

// 依赖注入
builder.Services.AddSingleton<Carder>();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSignalR();

builder.Services.AddControllers();

var app = builder.Build();

// 启用 CORS
app.UseCors("AllowAll");

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<GameHub>("/gamehub");
});

app.Run();
