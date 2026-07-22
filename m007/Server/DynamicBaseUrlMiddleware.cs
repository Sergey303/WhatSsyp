using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class DynamicBaseUrlMiddleware
{
    private readonly RequestDelegate _next;

    public DynamicBaseUrlMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Проверяем, что запрос идет к HTML-файлу
        if (context.Request.Path.Value!.EndsWith(".html") || context.Request.Path.Value == "/")
        {
            var originalBodyStream = context.Response.Body;

            using var responseBodyMemoryStream = new MemoryStream();
            context.Response.Body = responseBodyMemoryStream;

            await _next(context);

            // Модифицируем ответ только если это HTML-контент
            if (context.Response.ContentType?.StartsWith("text/html") == true)
            {
                context.Response.Body = originalBodyStream;

                responseBodyMemoryStream.Seek(0, SeekOrigin.Begin);
                var html = await new StreamReader(responseBodyMemoryStream).ReadToEndAsync();

                // Формируем базовый URL (учитывая подпапки IIS)
                var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}";
                var baseTag = $"<base href=\"{baseUrl}/\">";

                // Автоматически вставляем тег <base> в начало секции <head>
                if (html.Contains("<head>", StringComparison.OrdinalIgnoreCase))
                {
                    html = html.Replace("<head>", $"<head>\n    {baseTag}", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    // Если тега <head> нет, вставляем в самое начало файла
                    html = baseTag + html;
                }

                // Записываем измененный HTML обратно в ответ
                var bytes = Encoding.UTF8.GetBytes(html);
                context.Response.ContentLength = bytes.Length;
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            }
            else
            {
                // Если это не HTML (например картинка или js), возвращаем поток как есть
                responseBodyMemoryStream.Seek(0, SeekOrigin.Begin);
                await responseBodyMemoryStream.CopyToAsync(originalBodyStream);
            }
        }
        else
        {
            await _next(context);
        }
    }
}
