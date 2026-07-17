using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text;
using System.Text.Json;

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

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});

List<Room> rooms = new List<Room>();

app.MapGet("/api/rooms", () => rooms);

app.MapPost("/api/rooms", (Room room) =>
{
    rooms.Add(room); 
    return Results.Ok();
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
    
    string relativePath = filePath.Replace(Directory.GetCurrentDirectory(), "").Replace("\\", "/").TrimStart('/');
    return Results.Ok(relativePath);
}).DisableAntiforgery();

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
    return Results.Ok(new { name = name});
});

app.MapPost("api/register", (LoginRequest loginData, HttpContext context) =>
{
    try
    {
        string name = loginData.name;
        string login = loginData.login;
        string password = loginData.password;
        if (accountsList.Any(a => a.name == name || a.login == login))
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

app.MapPost("/api/logout", async (HttpContext context) =>
{
    await context.SignOutAsync();
    return Results.Ok();
});

app.MapPost("/api/login", async (LoginRequest loginData, HttpContext context) =>
{
    if (!accountsList.Any(a => a.login == loginData.login && a.password == loginData.password))
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

    ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    ClaimsPrincipal user = new ClaimsPrincipal(identity);

    await context.SignInAsync(user);

    return Results.Ok();
});

app.MapHub<ChatHub>("/ChatHub");

app.MapGet("/", async () => 
{
    var path = Path.Combine(builder.Environment.WebRootPath, "reg.html");
    return Results.File(path, "text/html");
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