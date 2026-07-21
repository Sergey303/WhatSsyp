using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
// SignalR это типа библиотеки что помогает подключать нескольких клиентов. Тут просто "SignalR", так как это код для сервиса. (для клиента будет ".SignalR.Client)

// var builder = 
//     WebApplication.CreateBuilder(args);

// builder.Services.AddSignalR();
// // добавляет возможности SignalR в приложение.

// var app = 
//     builder.Build();

// app.MapGet(
//     "/",
//     () => "Server vorks");

// app.MapHub<ChatHub> (
//     "/chatHub");

// app.Run(
//     "http://0.0.0.0:5000");
    
public class ChatHub : Hub
{
    public Task Send(  // Путь шаг 2 (ChatHub.Send) (Путь шаг 3 - сервер)
             string eventName, 
             string text)
// события (То что пишется после объявения в скобках - параметр функции. Там могут указываться переменные. Данные переменные можно назвать "событиями" при этом в коде они никак отличаться не будут. То есть по факту технически это переменная, но мы сами решили что это "событие")
            
        {
            Console.WriteLine(
                "[" +
                eventName +
                "]" +
                text);
if (eventName == "gameStart")
{
            return Clients.All.SendAsync( 

                eventName,
                text);
}
            return Clients.All.SendAsync( 
// SendAsync - функция из библиоткки(?) Clients.All - объект, означает всех подключённых клиентов.

                eventName,
                text);
// Пояснение про ретерн - он отвечает за "возвращение" того что функция делает. То есть после параметров мы пишем что эта функция делает, и если нам надо чтобы она это выдала обратно - пишем "ретерн". Функции с "void" ничего не возвращают.
        }
}
