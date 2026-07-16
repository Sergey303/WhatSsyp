using System;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<ChatHub>("/chatHub");

string[] users = { "Masha", "Petya", "Ania", "Kirill" };
string[] chatUsers = { "Masha", "Petya" };

app.MapGet("/api/rooms", () => Rooms.rooms);
app.MapGet("/api/users", () => users);
app.MapGet("/api/chat-users", () => chatUsers);
app.MapGet("/api/rooms/{roomName}/users",  (string roomName) =>
{
    if (Rooms.usersByRoom.ContainsKey(roomName))
    {
        return Rooms.usersByRoom[roomName];
    }
    return Array.Empty<string>();
});

app.Run("http://0.0.0.0:8080");
