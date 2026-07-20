var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();
app.UseStaticFiles();

app.MapHub<ChatHub>("/chatHub");
app.MapGet("/", () => "Hello World!");

app.Run();
