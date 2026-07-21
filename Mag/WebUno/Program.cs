var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

var app = builder.Build();
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

app.MapHub<ChatHub>("/chatHub");
app.MapGet("/", () => Results.Content(
@"<!DOCTYPE html>
<html><head><meta charset='utf-8'></head>
<body> <h1>Старт WebUno</h1>
<a href='index1.html'>Запустить</a>
</body></html>
","text/html", System.Text.Encoding.UTF8));

app.Run();
