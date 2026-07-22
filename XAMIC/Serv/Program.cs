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
using System.Security.Cryptography;
using System.Text;



//user.json

// Users.Add(new LoginRequest() { Name = "Men1", Password = "1234", UserName = "Андрей" });
// string output1 = JsonSerializer.Serialize(Users);
// File.WriteAllText("DataUsers.json", output1);
// Users.Add(new LoginRequest() { Name = "Men2", Password = "1234", UserName = "Артем" });
// string output2 = JsonSerializer.Serialize(Users);
// File.WriteAllText("DataUsers.json", output2);
var builder = WebApplication.CreateBuilder(args);



builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();

builder.Services.AddSignalR();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/messages", () => File.ReadAllText("DataMessages.json"));


app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Context.Response.Headers.Append("Pragma", "no-cache");
            ctx.Context.Response.Headers.Append("Expires", "0");
        }
    }
});
app.MapGet("/", async () =>
{
    var path = Path.Combine(builder.Environment.WebRootPath, "reg.html");
    return Results.File(path, "text.html");
});
//rooms
app.MapGet("/api/rooms", () => JsonSerializer.Deserialize<List<Room>>(File.ReadAllText("Rooms.json")));
app.MapPost("/api/rooms", (Room room) =>
{
    List<Room> rooms = JsonSerializer.Deserialize<List<Room>>(File.ReadAllText("Rooms.json"));
    if (rooms.FirstOrDefault(x => x.name == room.name) == null)
    {
        rooms.Add(room);
        string convert = JsonSerializer.Serialize(rooms);
        File.WriteAllText("Rooms.json", convert);
    }
    return Results.Ok();
});

IResult entry(LoginRequest login, HttpContext context)
{
    List<LoginRequest> Users = JsonSerializer.Deserialize<List<LoginRequest>>(File.ReadAllText("DataUsers.json"));
    if (Users.FirstOrDefault(x => x.Name == login.Name && x.Password == HashCompute.Compute256(login.Password) && x.UserName == login.UserName) == null)
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
}

app.MapPost("/olele", (LoginRequest request, HttpContext context) =>
{
    List<LoginRequest> Users = JsonSerializer.Deserialize<List<LoginRequest>>(File.ReadAllText("DataUsers.json"));
    if (Users.FirstOrDefault(x => x.UserName == request.UserName) == null)
    {
        string HashPassword = HashCompute.Compute256(request.Password);
        Users.Add(new LoginRequest() { Name = request.Name, Password = HashPassword, UserName = request.UserName });
        string ser = JsonSerializer.Serialize(Users);
        File.WriteAllText("DataUsers.json", ser);
        return entry(request, context);
    }
    else
    {
        return Results.Unauthorized();
    }
    
});
app.MapPost("api/login", (LoginRequest login, HttpContext context) =>
{
    return entry(login, context);
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


app.MapHub<ChatHub>("/ChatHub");
app.Run("http://0.0.0.0:8080");





    


