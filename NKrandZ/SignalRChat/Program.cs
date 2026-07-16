using System;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<ChatHub>("/chatHub");

Rooms.usersByRoom["General"] = new[] {};
Rooms.usersByRoom["Games"] = new[] {};
Rooms.usersByRoom["School"] = new[] {};

app.MapGet("/api/rooms", () => Rooms.rooms);
app.MapGet("/api/rooms/{roomName}/users",  (string roomName) =>
{
    if (Rooms.usersByRoom.ContainsKey(roomName))
    {
        return Rooms.usersByRoom[roomName];
    }
    return Array.Empty<string>();
});

app.Run("http://0.0.0.0:8080");
