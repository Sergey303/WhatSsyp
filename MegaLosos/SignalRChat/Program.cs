using System.Text;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

Console.WriteLine("start");
var processor = new AccountProcessor();
var accountsList = processor.LoadAccounts();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

builder.Services.AddAntiforgery(options => 
{
    options.SuppressXFrameOptionsHeader = true;
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<ChatHub>("/chatHub");

app.MapGet("/api/me", (HttpContext context) =>
{
    string name = "";
    // string login = "";

    if (context.User.Identity != null && context.User.Identity.Name != null)
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
    Console.WriteLine(filePath.Split("\\").Last());
    //C:\IAS\WhatSsyp\MegaLosos\SignalRChat\uploads\8523ed99-28d6-4eb5-86fa-b4ed3d00983d\x6jc2gc68ph81.mp4
    //return Results.File(Path.Combine(Directory.GetCurrentDirectory(), "uploads", filePath), "", );
    return Results.File(filePath, "application/octet-stream", filePath.Split("\\").Last());
});

app.MapPost("/api/logout", (HttpContext context) =>
{
    context.SignOutAsync().Wait();
    return Results.Ok();
});

app.MapPost("/api/login", async (LoginRequest loginData, HttpContext context) =>
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

app.MapPost("api/register", (LoginRequest loginData, HttpContext context) =>
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

app.MapPost("api/upload", async (IFormFile file) =>
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

app.Run("http://0.0.0.0:8080");



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