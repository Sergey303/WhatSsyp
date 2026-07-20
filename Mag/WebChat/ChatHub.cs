using Microsoft.AspNetCore.SignalR;
public class ChatHub : Hub
{
    public async Task SendMessage(string command, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", command, message);
    }
}