using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    // Метод, который клиенты могут вызывать для отправки сообщений
    public async Task SendMessage(string command, string message)
    {
        // Рассылает сообщение всем подключенным клиентам
        await Clients.All.SendAsync("ReceiveMessage", command, message);
    }
}