var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// app.MapGet("/", () => "Hello World!");

// Без этой строки ASP.NET Core не отдаст read.html и read.js из папки wwwroot.
app.UseStaticFiles();
app.UseDefaultFiles();

// Собираем абсолютный путь к книге независимо от папки, из которой запущена программа.
var bookPath = Path.Combine(
    // ContentRootPath указывает на корневую папку ASP.NET Core-проекта.
    app.Environment.ContentRootPath,
    // Добавляем папку, в которой хранятся книги.
    "Books",
    // Добавляем имя JSON-файла, который будет доступен по /api/book.
    "memory-city.json");

// Связываем GET-запрос /api/book с подготовленным JSON-файлом.
app.MapGet("/api/book", () =>
    // application/json сообщает браузеру формат ответа.
    Results.File(bookPath, "application/json"));

app.Run( // эта строка всегда в конце
    "http://0.0.0.0:5001"); // Основной адрес