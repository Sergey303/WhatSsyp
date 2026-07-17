using Microsoft.AspNetCore.SignalR.Client;
string serverUrl = "http://localhost:5000";
HubConnection connection =
new HubConnectionBuilder.WithUrl(serverUrl + "/chatHub").Build();
Console.WriteLine("Подключаемся...");
connection.StartAsync().Wait();
Console.WriteLine("Подключились");
Console.ReadLine();
connection.StopAsync().Wait();





