using Microsoft.AspNetCore.SignalR.Client;

string serverUrl = "https://localhost:7000";
HubConnection connection = new HubConnectionBuilder().WithUrl(serverUrl + "/chatHub").WithAutomaticReconnect().Build();
connection.On<string>("ReceiveClientIp", (ipAddress) =>
{
    Console.WriteLine($"Ваш IP-адрес (определен сервером): {ipAddress}");
});
Console.WriteLine("Подключаемся к серверу...");
try {
    await connection.StartAsync();
    Console.WriteLine("Успешно подключено к серверу!");
} catch (Exception ex) {
    Console.WriteLine($"Ошибка при подключении: {ex.Message}");
}
Console.WriteLine("Нажми Enter, чтобы закрыть клиент.");
Console.ReadLine();
await connection.StopAsync();