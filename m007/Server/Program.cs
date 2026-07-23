using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json;
using System.Text;

var processor = new AccountProcessor();
var accountsList = processor.LoadAccounts();
var builder = WebApplication.CreateBuilder(args);
// 1. Register CORS service and define a policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();

builder.Services.AddSignalR();
builder.Services.AddHostedService<MyBackGroundService>();

var app = builder.Build();
app.UseMiddleware<DynamicBaseUrlMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseCors("AllowAll");
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

//часть Александра
app.MapGet("api/rooms", () => Room.rooms.Select(x => x.name));

app.MapGet("api/file", (string filePath) =>
{
    string fileFullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
    return Results.File(fileFullPath, "application/octet-stream", filePath.Split("\\").Last());
});

//end MEGA LOSOS...

//app.MapGet("/", () => "Hello World!");

///;

// app.UseStaticFiles(new StaticFileOptions
// {
//     FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
//         Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
//     RequestPath = "/uploads"
// });



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

//end MEGA LOSOS...
app.MapGet("/api/MyTasks", async () =>
{
    List<MyTask> lo = await MyTask.GetListOfTaskAsync();
    string json = JsonSerializer.Serialize(lo);
    return json;
});

//app.MapGet("/", () => "Hello World!");
app.MapHub<ChatHub>("/chatHub");
app.MapHub<ScheduleHub>("/scheduleHub");




app.MapPost("api/logout", async (HttpContext context) =>
{
    await context.SignOutAsync();
    return Results.Ok();
});
app.MapGet("api/messages/{room?}", (string? room) =>
{
    room = ""; //FIX LATER IF NEEDED
    List<Message> messages = JsonSerializer.Deserialize<List <Message>>(FileExtensions.ReadAllTextSafe("DataMessages.json", "[]"));
    return messages.Where(a => a.room == room).ToList();
});

app.MapPost("api/register", async (LoginRequest loginData, HttpContext context) =>
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
        await AddAccountToList(name, login, password, context);
        AddAccountToFile(name, login, password);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.Problem();
    }
});
app.MapGet("api/me", (HttpContext context) =>
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
app.MapPost("api/login", async (LoginRequest loginData, HttpContext context) =>
{
    string inputHash = HashCompute.Compute256(loginData.password);

    if (!accountsList.Any(a => a.login == loginData.login && a.password == inputHash))
    {
        return Results.Unauthorized();
    }
    Account? _logAcc = accountsList.Find(a => a.login == loginData.login);

    await LoginAfterRegister(_logAcc, context);

    return Results.Ok();
});

app.Run("http://0.0.0.0:8080");

void SaveRooms(List<Room> roomsList)
{
    try
    {
        string path = "wwwroot/rooms.json";
        string json = JsonSerializer.Serialize(roomsList);
        File.WriteAllText(path, json, Encoding.UTF8);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Ошибка сохранения комнат: " + ex.Message);
    }
}

async Task AddAccountToList(string name, string login, string password, HttpContext httpContext)
{
    string hashPassword = HashCompute.Compute256(password);
    Account _acc = new Account {name = name, login = login, password = hashPassword};
    await LoginAfterRegister(_acc, httpContext);
    accountsList.Add(_acc);
}

void AddAccountToFile(string name, string login, string password)
{
    string jsn = JsonSerializer.Serialize(accountsList);
    File.WriteAllText(AccountProcessor.filePath, jsn, Encoding.UTF8);
}

async Task LoginAfterRegister(Account? logAcc, HttpContext httpContext)
{
    Claim nameClaim = new Claim(ClaimTypes.Name, logAcc.name);
    Claim loginClaim = new Claim(ClaimTypes.NameIdentifier, logAcc.login);
    Claim passwordClaim = new Claim(ClaimTypes.SerialNumber, logAcc.login);
    List<Claim> claims = new();
    claims.Add(nameClaim);
    claims.Add(loginClaim);
    claims.Add(passwordClaim);

    ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    ClaimsPrincipal user = new ClaimsPrincipal(identity);

    await httpContext.SignInAsync(user);
}
