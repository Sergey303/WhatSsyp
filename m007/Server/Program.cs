using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IO;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Установка заголовков для .html и .js файлов
        if (ctx.File.Name.EndsWith(".html") || ctx.File.Name.EndsWith(".js")
         || ctx.File.Name.EndsWith(".css"))
        {
            ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers.Pragma = "no-cache";
            ctx.Context.Response.Headers.Expires = "-1";
        }
    }
});

string[] users = { "Masha", "Petya", "Ania", "Kirill" };
string[] chatUsers = { "Masha", "Petya" };

Rooms.usersByRoom["General"] = new string[] {};
Rooms.usersByRoom["Games"] = new string[] {};
Rooms.usersByRoom["School"] = new string[] {};

Rooms.messagesByRoom["General"] = new List<ChatMessage> {};
Rooms.messagesByRoom["Games"] = new List<ChatMessage> {};
Rooms.messagesByRoom["School"] = new List<ChatMessage> {};

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

//app.MapGet("/", () => "Hello World!");
app.MapHub<ChatHub>("/chatHub");
app.MapGet("/", () => "/indexAndrey.html");
app.Run("http://0.0.0.0:8080");
