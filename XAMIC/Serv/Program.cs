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



List<Room> rooms = new List<Room>();
Room general = new Room();


List<LoginRequest> Users = JsonSerializer.Deserialize<List<LoginRequest>>(File.ReadAllText("DataUsers.json"));
Users.Add(new LoginRequest() { Name = "Men1", Password = "1234", UserName = "Андрей" });
string output = JsonSerializer.Serialize(Users);
File.WriteAllText("DataUsers.json", output);
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();

builder.Services.AddSignalR();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/rooms", () => rooms);
app.MapPost("/api/rooms", (Room room) =>
{
    rooms.Add(room); return Results.Ok();
});
app.MapPost("/olele", (LoginRequest request) =>
{
    if (!Users.FirstOrDefault(x=>x.UserName==request.UserName))
    {
        Users.Add(new LoginRequest() { Name = request.Name, Password = request.Password, UserName = request.UserName});
        string ser = JsonSerializer.Serialize(Users);
        File.WriteAllText("DataUsers.json", ser);
        return Results.Ok();
    }
    else
    {
        return Results.Unauthorized();
    }
    
});
app.MapPost("api/login", (LoginRequest login, HttpContext context) =>
{
    if (users_password.ContainsKey(login.Name) == false)
    {
        return Results.Unauthorized();
    }
    if (users_password[login.Name] != login.Password)
    {
        return Results.Unauthorized();
    }
    if (users_username.ContainsKey(login.Name) == false)
    {
        return Results.Unauthorized();
    }
    if (users_username[login.Name] != login.UserName)
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

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<ChatHub>("/ChatHub");
app.Run("http://0.0.0.0:8080");





    


