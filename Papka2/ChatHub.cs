using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text;
using System.Security.Claims;

public class ChatHub : Hub
{
    private static Dictionary<string, List<string>> roomMembers = new Dictionary<string, List<string>>();
    private static Dictionary<string, List<ChatMessage>> roomMessages = new Dictionary<string, List<ChatMessage>>();
    private static Dictionary<string, string> connectionRooms = new Dictionary<string, string>();
    private static string messagesFilePath = "wwwroot/messages.json";

    static ChatHub()
    {
        LoadMessages();
    }

    private static void LoadMessages()
    {
        try
        {
            if (File.Exists(messagesFilePath))
            {
                string json = File.ReadAllText(messagesFilePath, Encoding.UTF8);
                var data = JsonSerializer.Deserialize<Dictionary<string, List<ChatMessage>>>(json);
                if (data != null)
                {
                    roomMessages = data;
                }
            }
        }
        catch
        {
            roomMessages = new Dictionary<string, List<ChatMessage>>();
        }
    }

    private static void SaveMessages()
    {
        try
        {
            string json = JsonSerializer.Serialize(roomMessages);
            File.WriteAllText(messagesFilePath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка сохранения сообщений: " + ex.Message);
        }
    }

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
        else if (eventName == "file")
        {
            await SendFileMessage(text);
        }
        else if (eventName == "voice")
        {
            await SendVoiceMessage(text);
        }
        else
        {
            await Clients.All.SendAsync(eventName, text);
        }
    }

    public async Task SendAudioData(float[] audioData)
    {
        string roomName = GetRoomName();
        string userName = GetUserName();
        await Clients.OthersInGroup(roomName).SendAsync("ReceiveAudio", audioData, userName);
    }

    private async Task SendChat(string text)
    {
        string name = GetUserName();
        if (string.IsNullOrEmpty(name))
        {
            await Clients.Caller.SendAsync("system", "Сначала войди");
            return;
        }

        string roomName = GetRoomName();
        
        var message = new ChatMessage
        {
            sender = name,
            text = text,
            time = DateTime.Now.ToString("HH:mm"),
            type = "text"
        };

        AddMessageToRoom(roomName, message);
        await Clients.Group(roomName).SendAsync("chat", name + ": " + text);
    }

    private async Task SendFileMessage(string json)
    {
        string name = GetUserName();
        if (string.IsNullOrEmpty(name))
        {
            await Clients.Caller.SendAsync("system", "Сначала войди");
            return;
        }

        string roomName = GetRoomName();
        
        var message = new ChatMessage
        {
            sender = name,
            text = json,
            time = DateTime.Now.ToString("HH:mm"),
            type = "file"
        };

        AddMessageToRoom(roomName, message);
        await Clients.Group(roomName).SendAsync("file", json);
    }

    private async Task SendVoiceMessage(string json)
    {
        string name = GetUserName();
        if (string.IsNullOrEmpty(name))
        {
            await Clients.Caller.SendAsync("system", "Сначала войди");
            return;
        }

        string roomName = GetRoomName();
        
        var message = new ChatMessage
        {
            sender = name,
            text = json,
            time = DateTime.Now.ToString("HH:mm"),
            type = "voice"
        };

        AddMessageToRoom(roomName, message);
        await Clients.Group(roomName).SendAsync("voice", json);
    }

    private string GetUserName()
    {
        if (Context.User != null && Context.User.Identity != null && Context.User.Identity.Name != null)
        {
            return Context.User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        }
        return "";
    }

    private string GetRoomName()
    {
        if (connectionRooms.ContainsKey(Context.ConnectionId))
        {
            return connectionRooms[Context.ConnectionId];
        }
        return "Общий";
    }

    private void AddMessageToRoom(string roomName, ChatMessage message)
    {
        if (!roomMessages.ContainsKey(roomName))
        {
            roomMessages[roomName] = new List<ChatMessage>();
        }
        roomMessages[roomName].Add(message);
        SaveMessages();
    }

    private async Task joinRoom(string json)
    {
        RoomJoin join = JsonSerializer.Deserialize<RoomJoin>(json) ?? new RoomJoin();
        if (join.RoomName == "")
        {
            await Clients.Caller.SendAsync("system", "Нужно название комнаты");
            return;
        }
        
        if (connectionRooms.ContainsKey(Context.ConnectionId))
        {
            string oldRoom = connectionRooms[Context.ConnectionId];
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, oldRoom);
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, join.RoomName);
        connectionRooms[Context.ConnectionId] = join.RoomName;
        
        if (!roomMembers.ContainsKey(join.RoomName))
        {
            roomMembers[join.RoomName] = new List<string>();
        }
        
        string userName = GetUserName();
        
        List<string> members = roomMembers[join.RoomName];
        if (!string.IsNullOrEmpty(userName) && !members.Contains(userName))
        {
            members.Add(userName);
        }
        
        if (roomMessages.ContainsKey(join.RoomName))
        {
            foreach (var msg in roomMessages[join.RoomName])
            {
                if (msg.type == "file")
                {
                    await Clients.Caller.SendAsync("file", msg.text);
                }
                else if (msg.type == "voice")
                {
                    await Clients.Caller.SendAsync("voice", msg.text);
                }
                else
                {
                    await Clients.Caller.SendAsync("chat", msg.sender + ": " + msg.text);
                }
            }
        }
        
        string membersJson = JsonSerializer.Serialize(members);
        await Clients.Group(join.RoomName).SendAsync("roomMembers", membersJson);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (connectionRooms.ContainsKey(Context.ConnectionId))
        {
            connectionRooms.Remove(Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
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

public class ChatMessage
{
    public string sender { get; set; } = "";
    public string text { get; set; } = "";
    public string time { get; set; } = "";
    public string type { get; set; } = "text";
}