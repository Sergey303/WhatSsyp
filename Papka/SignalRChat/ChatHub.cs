using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using System.Security.Claims;

public class ChatHub : Hub
{
    
    private static Dictionary<string, List <string>> roomMembers = new Dictionary<string, List<string>>();
    public Task Send(string eventName, string text)
    {
        if (eventName == "chat")
        {
            return SendChat(text);
        }
        if (eventName == "joinRoom")
        {
            return joinRoom(text);
        }
        return Clients.All.SendAsync(eventName, text);
    }
    private Task SendChat(string text)
    {
        string name = "";
        if (Context.User != null && Context.User.Identity != null && Context.User.Identity.Name != null)
        {
            //name = Context.User.Claims.FirstOrDefault((x)=>((x.Type == ClaimTypes.UserData)))?.Value;
            string userName = "UserName";
            name = Context.User.FindFirst(userName).Value;
        }
        
        if (name == "")
        {
            return Clients.Caller.SendAsync("system", "Сначала войди");
        }
        return Clients.All.SendAsync("chat", name + ": " + text);
    }
    private Task joinRoom(string json)
    {
        RoomJoin join = JsonSerializer.Deserialize<RoomJoin>(json) ?? new RoomJoin();
        if (join.RoomName == "" || join.UserName == "")
        {
            return Clients.Caller.SendAsync("system", "нужное имя и название комнаты");
        }
        Groups.AddToGroupAsync(Context.ConnectionId, join.RoomName).Wait();
        if (roomMembers.ContainsKey(join.RoomName) == false)
        {
            roomMembers[join.RoomName] = new List<string>();
        }
        List<string> members = roomMembers[join.RoomName];
        if (members.Contains(join.UserName) == false)
        {
            members.Add(join.UserName);
        }
        string membersJson = JsonSerializer.Serialize(members);
        return Clients.Group(join.RoomName).SendAsync("roomMembers", membersJson);
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