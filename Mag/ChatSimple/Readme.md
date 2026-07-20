# Создаем простой мессенджер (или чат)

## Наша задача
Надо создать Веб-конструкцию типа чата (Telegram, Max и др.). Чат будет иметь несколько комнат, а в каждой комнате какое-то 
содержимое. Канонической комнатой с содержимым является обмен сообщениями, когда пользователь видит пришедшие сообщения
и может послать свое, которое примут все подключенные пользователи. Сообщение, в общем случае html-композиция. 

Минимизируем задачу. 
1. Комнат фиксированный набор, комната определяется своим названием.
2. Сообщения - композиция из фотографии, которой может и не быть и текста.
3. Пользователь - любой, заявивший о себе при входе

## Схема решения
Имеется Minimal API построение. Оно используется в качестве носителя Хаба технологии SignalR. Также оно используется 
для организации простых сервисов, необходимых в обработке. Основой решения является Хаб (Hub), который принимает от клиента
запрос и возвращает всем клиентам новое сообщение.
Простая главная программа:
```
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Добавляем SignalR в сервисы
builder.Services.AddSignalR();

var app = builder.Build();

// Маршрутизируем запросы к нашему чат-хабу
app.MapHub<ChatHub>("/chatHub");

app.Run();
```
А стартовая версия (основа) хаба:
```
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    // Метод, который клиенты могут вызывать для отправки сообщений
    public async Task SendMessage(string command, string jmessage)
    {
        // Рассылает сообщение всем подключенным клиентам
        await Clients.All.SendAsync("ReceiveMessage", command, jmessage);
    }
}
```
При этом, command - команда, которая наверняка понадобится, jmessage - JSON-конверт с сообщением. 
Формат JSON-пакета следующий:
```
{ "user": имя-пользователя, "group": комната, "message": сообщение, "dt": время }
```

Клиент реализуется как HTML-файл с JavaScript-кодом (и CSS). Средствами HTML создаем прототип 
главной страницы мессенджера, напр.:
```
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.css">
</head>
<body>
    <div class="container">
        <div class="row p-1">
            <div class="col-6"><input type="text" id="userInput" placeholder="User" /></div>
        </div>

        <div class="row p-1">
            <select class="form-select col-1" size="6" style="width:200px;height:250px;">
                <option value="1" selected>Один</option>
                <option value="2">Два</option>
                <option value="3">Три</option>
            </select>
            <div class="overflow-auto border" style="height: 150px; width: 300px; height: 250px;">
                <ul class="list-group list-group-flush">
                    <li class="list-group-item">Элемент 1</li>
                    <li class="list-group-item">Элемент 2</li>
                    <li class="list-group-item">Элемент 3</li>
                    <li class="list-group-item"> Элемент 4 Элемент 4 Элемент 4 Элемент 4 Элемент 4 Элемент 4</li>
                    <li class="list-group-item">Элемент 5</li>
                </ul>
            </div>

        </div>

        <div class="row p-1">
            <div class="col-md-4">
                <input type="text" class="form-control" id="messageInput" placeholder="Message" />
            </div>
        </div>
        <div class="row p-1">
            <div class="col-md-4">
                <input type="file" class="form-control" id="messageInput" placeholder="Message" />
            </div>
        </div>
        <div class="row p-1">
            <div class="col-md-2" style="width:150px;">
                <input type="button" id="sendButton" value="Send message" />
            </div>
            <div class="col-md-2" style="width:150px;">
                <input type="button" id="clearButton" value="Clear message" />
            </div>
        </div>

    </div>

    <script src="js/signalr/dist/browser/signalr.js"></script>
    <script src="js/signalr/chat.js"></script>
</body>
</html>
```
Что здесь важно? Во-первых, страница обращается к внешним js и css файлам. Мы использовали
signalr.js, chat.js и bootstrap.css. Обратите внимание на то, чтобы используемые файлы были 
локализованы по указанным местам. Проект, скорее всего, загрузится, но надо добиться еще того,
чтобы он заработал, т.е. заработало и приложение и index.html и  ChatHub и скрипты. 




# Другой вариант

Создаем новый проект по шаблону razor

Упрощаем _Layout.cshtml

Изменяем Pages/Index.cshtml на подходящий дизайн. В конце вставляем:
```
<script src="~/js/signalr/dist/browser/signalr.js"></script>
<script src="~/js/chat.js"></script>
```
Обеспечиваем наличие файлов на нужных местах

Добавляем использование статических файлов:
```
app.UseStaticFiles(); 
```

Добавляем определение хаба (можно в отдельном файле):
```
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}

```

В программу добавить 
```
builder.Services.AddSignalR();

// и перед app.MapRazorPages
app.MapHub<ChatHub>("/chatHub");
```

Должно работать!...

### Попробую вернуться к MinimalApi
Создаю новые проект по шаблону web

Переношу код ChatHub

Создаю wwwroot путем переноса предыдущего решения

Делаю index.html так же как и раньше. Поместил его в wwwroot - все в порядке. 

Опять не получилось. Вроде даже работало, а потом все сломалось. На том же месте...

### Новая попытка. Проект WebUno

Создаю новые проект по шаблону web

