using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text;
using System.Security.Claims;

public class ChatHub : Hub
{
    private static Dictionary<string, List<string>> roomMembers = new Dictionary<string, List<string>>();
    private static Dictionary<string, List<string>> roomMessages = new Dictionary<string, List<string>>();

    public async Task Send(string eventName, string text)
    {
        if (eventName == "chat")
        {
            await SendChat(text);
        }
        else if (eventName == "joinRoom")
        {
            await joinRoom(text);
        }
        else if (eventName == "pct")
        {
            await Clients.All.SendAsync("pct", text);
        }
        else
        {
            await Clients.All.SendAsync(eventName, text);
        }
    }

    private async Task SendChat(string text)
    {
        string name = "";
        if (Context.User != null && Context.User.Identity != null && Context.User.Identity.Name != null)
        {
            name = Context.User.FindFirst(ClaimTypes.Name)?.Value;
        }

        if (string.IsNullOrEmpty(name))
        {
            await Clients.Caller.SendAsync("system", "Сначала войди");
            return;
        }
        
        string message = name + ": " + text;
        
        if (!roomMessages.ContainsKey("Общий"))
        {
            roomMessages["Общий"] = new List<string>();
        }
        roomMessages["Общий"].Add(message);
        
        await Clients.All.SendAsync("chat", message);
    }

    private async Task joinRoom(string json)
    {
        RoomJoin join = JsonSerializer.Deserialize<RoomJoin>(json) ?? new RoomJoin();
        if (join.RoomName == "")
        {
            await Clients.Caller.SendAsync("system", "Нужно название комнаты");
            return;
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, join.RoomName);
        
        if (!roomMembers.ContainsKey(join.RoomName))
        {
            roomMembers[join.RoomName] = new List<string>();
        }
        
        string userName = "";
        if (Context.User != null && Context.User.Identity != null)
        {
            userName = Context.User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        }
        
        List<string> members = roomMembers[join.RoomName];
        if (!string.IsNullOrEmpty(userName) && !members.Contains(userName))
        {
            members.Add(userName);
        }
        
        if (!roomMessages.ContainsKey(join.RoomName))
        {
            roomMessages[join.RoomName] = new List<string>();
        }
        
        foreach (var msg in roomMessages[join.RoomName])
        {
            await Clients.Caller.SendAsync("chat", msg);
        }
        
        string membersJson = JsonSerializer.Serialize(members);
        await Clients.Group(join.RoomName).SendAsync("roomMembers", membersJson);
    }

    public bool isIpBlocked(string name)
    {
        string jsnBlockedIPs = File.ReadAllText("wwwroot/BlockedIPs.json", Encoding.UTF8);
        List<string>? blockedIPs = JsonSerializer.Deserialize<List<string>>(jsnBlockedIPs);
        string? requestIP = this.Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
        Console.WriteLine($"{name}.   IP: {requestIP}");
        return blockedIPs != null && blockedIPs.Contains(requestIP);
    }
}