using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json;

public class ChatHub : Hub
{
    public Task Send(string eventName, string text)
    {
        string? name = this.Context.GetHttpContext().User?.Identity?.Name?.ToString();
        if (name == null)
        {
            return Clients.Caller.SendAsync("429");
        }
        if (isIpBlocked(name))
        {
            return Clients.Caller.SendAsync("429");
        }
        if (eventName == "chat")
        {
            return SendChat(text);
        }
        return Clients.All.SendAsync("chat", name + ": " + text);
    }

    private Task SendChat(string text)
    {
        string name = "";
        if (Context?.User?.Identity?.Name != null)
        {
            name = Context.User.Identity.Name;
        }

        if (name == "")
        {
            return Clients.Caller.SendAsync("system", "Сначала войди");
        }

        return Clients.All.SendAsync("chat", name + ": " + text);
    }

    public bool isIpBlocked(string name)
    {
        string jsnBlockedIPs = File.ReadAllText("wwwroot/BlockedIPs.json", Encoding.UTF8);
        List<string>? blockedIPs = JsonSerializer.Deserialize<List<string>>(jsnBlockedIPs);
        string? requestIP = this.Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
        Console.WriteLine($"{name}.   IP: {requestIP}");
        return blockedIPs.Contains(requestIP);
    }
}