using Microsoft.AspNetCore.SignalR.Client;


string serverUrl = "http://172.16.47.27:8080";
string userName = "GoyDa";

HubConnection connection = new HubConnectionBuilder()
    .WithUrl(serverUrl + "/chatHub")
    .Build();

connection.On<string>("chat", text =>
{
    Console.WriteLine(text);
});

connection.StartAsync().Wait();

Console.WriteLine("Подключение успешно!");
Console.WriteLine("Пиши Сообщения. Пустая строка и Enter - выход.");
while (true)
{
    string text = Console.ReadLine();

    if (text == "")
    {
        break;
    }

    string message = userName + ": " + text;

    connection.InvokeAsync("Send", "chat", message).Wait();
}

connection.StopAsync().Wait();