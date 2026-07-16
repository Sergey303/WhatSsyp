using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic; // библиотека для обработки списков (ради List<t (тип, как инт или стринг)>)
using System.Text.Json; 
    // ради вот этого --> (то есть ради конвертации colorField(объект) в json (строка))
    // string json =
    //     JsonSerializer.Serialize(
    //         colorField);

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
private Task StartColorGame()
{
    string task;

    lock (gameLock)
    {
        if (colorGameActive == false)
        {
            colorCurrentTask =
                CreateSharedColorTask();

            colorGameActive = true;
        }

        task = colorCurrentTask;
    }

    string json = 
        JsonSerializer.Serialize(
            colorField); //  конвертация colorField(объект) в json (строка)
 
    Clients.All
        .SendAsync(
            "colorField",
            json)
        .Wait();

    return Clients.All.SendAsync(
        "colorTask",
        task);
}

// 
private static Random colorRandom = // объект. генератор случайных чисел (мы задали этому объекту/полю значение Random)
    new Random();

private static List<string> colorField = // поле(то есть это "кусочек" объекта. название - colorField, тип - поле ), значение коороего есть список строк
    new List<string>
    {
        "Красный",
        "Синий",
        "Зелёный",
        "Жёлтый",
        "Белый",
        "Чёрный"
    };

private static string colorCurrentTask = "";
private static bool colorGameActive = false;

private string CreateSharedColorTask()
{
        int index =
            colorRandom.Next(
                0,
                colorField.Count);

    return colorField[index] + "и тд";
}
    public Task Send(  // Путь шаг 2 (ChatHub.Send) (Путь шаг 3 - сервер)

             string eventName, 
             string text)
// события (То что пишется после объявения в скобках - параметр функции. Там могут указываться переменные. Данные переменные можно назвать "событиями" при этом в коде они никак отличаться не будут. То есть по факту технически это переменная, но мы сами решили что это "событие")
             
        {
if (eventName == "colorStart")
{
    return StartColorGame();
}

if (eventName == "colorClick")
{
    return ColorClick(text);
}

            Console.WriteLine(
                "[" +
                eventName +
                "]" +
                text);

            return Clients.All.SendAsync( 
// SendAsync - функция из библиоткки(?) Clients.All - объект, означает всех подключённых клиентов.

                eventName,
                text);
// Пояснение про ретерн - он отвечает за "возвращение" того что функция делает. То есть после параметров мы пишем что эта функция делает, и если нам надо чтобы она это выдала обратно - пишем "ретерн". Функции с "void" ничего не возвращают.
        }
}
