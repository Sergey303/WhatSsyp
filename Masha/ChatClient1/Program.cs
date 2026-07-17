using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/chatHub") // Replace with your server URL
    .WithAutomaticReconnect()                 // Handles dropouts automatic
    .Build();
connection.On<string>("chat", text =>
{
    Console.WriteLine(text);
});

Console.WriteLine("Подключаемся...");
connection.StartAsync().Wait();
Console.WriteLine("Подключились");
Console.WriteLine("Как тебя зовут?");
string userName = Console.ReadLine() ?? "";
if (userName == "")
{
    userName = "Игрок";
}
Console.WriteLine("Пиши сообщения.");
Console.WriteLine("Пустая строка - выход.");

while (true)
{
    string text = Console.ReadLine() ?? "";
    if (text == "")
    {
        break;
    }
    string message = userName + ":" + text;
    connection.InvokeAsync("Send", "chat", message).Wait();
}
connection.StopAsync().Wait();

