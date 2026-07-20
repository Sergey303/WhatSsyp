var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Content(
@"<!DOCTYPE html>
<html><head><meta charset='utf-8'></head>
<body> <h1>Старт WebUno</h1>
</body></html>
","text/html", System.Text.Encoding.UTF8));

app.Run();
