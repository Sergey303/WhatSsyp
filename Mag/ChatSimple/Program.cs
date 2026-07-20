using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Добавляем SignalR в сервисы
builder.Services.AddSignalR();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Маршрутизируем запросы к нашему чат-хабу
app.MapHub<ChatHub>("/chatHub");


app.Run();