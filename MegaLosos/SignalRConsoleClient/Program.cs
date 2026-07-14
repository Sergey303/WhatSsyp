using Microsoft.AspNetCore.SignalR.Client;
using System;

string serverUrl = "http://localhost:5083";
string userName = "Losos";

HubConnection connection = new HubConnectionBuilder()
    .WithUrl(serverUrl + "/chatHub")
    .Build();

connection.On<string>("chat", text =>
{
    Console.WriteLine(text);
});

connection.StartAsync().Wait();

Console.WriteLine("Клиент подключён.");
Console.WriteLine("Пиши сообщения. Пустая строка и Enter — выход.");

while (true)
{
    var text = Console.ReadLine();

    if (text == "" || text == null)
    {
        break;
    }

    string message = userName + ": " + text;

    connection.InvokeAsync("Send", "chat", message).Wait();
}

connection.StopAsync().Wait();