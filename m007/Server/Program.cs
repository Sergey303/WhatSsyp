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
using System.Text;

List<Room> rooms = JsonSerializer.Deserialize<List<Room>>(File.ReadAllText("RoomsAlexander.json"));

List<LoginRequest> Users = JsonSerializer.Deserialize<List<LoginRequest>>(File.ReadAllText("DataUsersAlexander.json"));

var processor = new AccountProcessor();
var accountsList = processor.LoadAccounts();
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
    List<Claim> claims = new List<Claim> {nameClaim1, nameClaim2};

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

//MEGA LOSOS!!!!!!
app.MapGet("/api/fish", (HttpContext context) =>
{
    string name = "";
    // string login = "";

    if (context.User?.Identity?.Name != null)
    {
        name = context.User.Identity.Name;
        // var claims = context.User.Claims;
        // login = claims.FirstOrDefault(a =>
        //     a.GetType().ToString() == ClaimTypes.NameIdentifier).Value;
    }

    if (name == "")
    {
        return Results.Unauthorized();
    }
    return Results.Ok(new { name = name});
});

app.MapGet("api/MLfile", (string filePath) =>
{
    // Console.WriteLine(filePath.Split("\\").Last());
    return Results.File(filePath, "application/octet-stream", filePath.Split("\\").Last());
});

app.MapPost("/api/MLlogout", (HttpContext context) =>
{
    context.SignOutAsync().Wait();
    return Results.Ok();
});

app.MapPost("/api/MLlogin", async (LoginRequest2 loginData, HttpContext context) =>
{
    if (!accountsList.Any(a =>
    a.login == loginData.login && a.password == loginData.password))
    {
        return Results.Unauthorized();
    }

    Account? _logAcc = accountsList.Find(a => a.login == loginData.login);

    Claim nameClaim = new Claim(ClaimTypes.Name, _logAcc.name);
    Claim loginClaim = new Claim(ClaimTypes.NameIdentifier, _logAcc.login);
    Claim passwordClaim = new Claim(ClaimTypes.SerialNumber, _logAcc.login);
    List<Claim> claims = new();
    claims.Add(nameClaim);
    claims.Add(loginClaim);
    claims.Add(passwordClaim);

    ClaimsIdentity identity = new ClaimsIdentity(claims,
        CookieAuthenticationDefaults.AuthenticationScheme);
    ClaimsPrincipal user = new ClaimsPrincipal(identity);

    await context.SignInAsync(user);

    return Results.Ok();
});

app.MapPost("api/MLregin", (LoginRequest2 loginData, HttpContext context) =>
{
    try
    {
        string name = loginData.name;
        string login = loginData.login;
        string password = loginData.password;
        if (name.Length > 20 || login.Length > 20 || password.Length > 20)
        {
            return Results.BadRequest();
        }
        if (accountsList.Any(a => a.name == name ||
            a.login == login))
        {
            Console.WriteLine(name, login);
            return Results.BadRequest();
        }
        AddAccountToList(name, login, password);
        AddAccountToFile(name, login, password);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.Problem();
    }
});

app.MapPost("api/MLupload", async (IFormFile file) =>
{
    string dir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", Guid.NewGuid().ToString());
    string filePath = Path.Combine(dir, file.FileName);
    Directory.CreateDirectory(dir);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }
    return Results.Ok(filePath);
}).DisableAntiforgery();
//end MEGA LOSOS...


//app.MapGet("/", () => "Hello World!");
app.MapHub<ChatHub>("/chatHub");
app.Run();


void AddAccountToList(string name, string login, string password)
{
    Account _acc = new Account {name = name, login = login, password = password};
    accountsList.Add(_acc);
}

void AddAccountToFile(string name, string login, string password)
{
    string jsn = JsonSerializer.Serialize(accountsList);
    File.WriteAllText(AccountProcessor.filePath, jsn, Encoding.UTF8);
}