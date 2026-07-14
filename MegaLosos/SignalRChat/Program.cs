using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Http.HttpResults;

var processor = new AccountProcessor();
var accountsList = processor.LoadAccounts();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthentication();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
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

app.MapPost("/api/logout", (HttpContext context) =>
{
    context.SignOutAsync().Wait();
    return Results.Ok();
});

app.MapPost("/api/login", (LoginRequest loginData, HttpContext context) =>
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

    context.SignInAsync(user).Wait();

    return Results.Ok();
});

app.MapPost("api/register", (LoginRequest loginData, HttpContext context) =>
{
    try
    {
        string name = loginData.name;
        string login = loginData.login;
        string password = loginData.password;
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