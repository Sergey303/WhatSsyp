using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.ComponentModel;
using System.Net.Cache;

List<Room> rooms = JsonSerializer.Deserialize<List<Room>>(File.ReadAllText("RoomsAlexander.json"));

List<LoginRequest> Users = JsonSerializer.Deserialize<List<LoginRequest>>(File.ReadAllText("DataUsersAlexander.json"));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();

builder.Services.AddSignalR();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
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

//часть Александра
app.MapGet("/api/roomsAlexander", () => rooms);
app.MapPost("/api/roomsAlexander", (Room room) =>
{
    if (rooms.FirstOrDefault(x => x.name == room.name) == null)
    {
        rooms.Add(room);
        string convert = JsonSerializer.Serialize(rooms);
        File.WriteAllText("RoomsAlexander.json", convert);
    }
    return Results.Ok();
});


app.MapPost("/olele", (LoginRequest request) =>
{
    if (Users.FirstOrDefault(x=>x.UserName==request.UserName)==null)
    {
        Users.Add(new LoginRequest() { Name = request.Name, Password = request.Password, UserName = request.UserName});
        string ser = JsonSerializer.Serialize(Users);
        File.WriteAllText("DataUsersAlexander.json", ser);
        return Results.Ok();
    }
    else
    {
        return Results.Unauthorized();
    }
    
});
app.MapPost("api/login", (LoginRequest login, HttpContext context) =>
{
    if (Users.FirstOrDefault(x=>x.Name == login.Name && x.Password == login.Password && x.UserName == login.UserName)==null)
    {
        return Results.Unauthorized();
    }
    string userName = "UserName";

    Claim nameClaim1 = new Claim(ClaimTypes.Name, login.Name);
    Claim nameClaim2 = new Claim(userName, login.UserName);
    List<Claim> claims = [nameClaim1, nameClaim2];

    ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    ClaimsPrincipal user = new ClaimsPrincipal(identity);
    context.SignInAsync(user).Wait();
    return Results.Ok();
});
app.MapGet("/api/me", (HttpContext context) =>
{
    string name = "";
    if (context.User.Identity != null && context.User.Identity.Name != null)
    {
        name = context.User.Identity.Name;
    }
    if (name == "")
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new { name = name });
});
app.MapPost("/api/logout", (HttpContext context) => { context.SignOutAsync().Wait(); return Results.Ok(); });





//app.MapGet("/", () => "Hello World!");
app.MapHub<ChatHub>("/chatHub");
app.Run("http://0.0.0.0:8080");
