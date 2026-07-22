using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Security.Claims;
using System.Text;


public class ChatHub : Hub {
    
    public Task Send(string eventName, string jtext) {
        
        Console.WriteLine($"event name: {eventName} jtext: {jtext}");
        if (IsIpBlocked())
        {
            return Clients.Caller.SendAsync("429");
        }

        else if (eventName == "chat")
        {
          return SendChat(jtext);
        }
        else if (eventName == "joinRoom")
        {
            return JoinRoom(jtext);
        }
        else if (eventName == "createRoom")
        {
            return CreateRoom(jtext);
        }

        return Clients.All.SendAsync(eventName, jtext);
    }
    private Task SendChat(string jtext)
    {
        string name = GetUserName();
        if (string.IsNullOrEmpty(name))
        {
            return Clients.Caller.SendAsync("system", "Сначала войди");
        }
        var record = JsonSerializer.Deserialize<DbRecord>(jtext);
        record.user = name;
        var db = DbRecord.Records;
        record.dt = DateTime.Now.ToString();
        db.Add(record);
        var newDbString = JsonSerializer.Serialize(db, new JsonSerializerOptions(){WriteIndented=true});
        File.WriteAllText("DataBase.json", newDbString);
            var newMessage = JsonSerializer.Serialize(record, new JsonSerializerOptions(){WriteIndented=true});
        if(string.IsNullOrEmpty(record.group))
        {
            return Clients.All.SendAsync("chat",  newMessage );
        }
        else
            return Clients.Group(record.group).SendAsync("chat", newMessage);
        
    }

    public bool IsIpBlocked()
    {
        string jsnBlockedIPs = FileExtensions.ReadAllTextSafe("BlockedIPs.json", "[]");
        List<string>? blockedIPs = JsonSerializer.Deserialize<List<string>>(jsnBlockedIPs);
        string? requestIP = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
        return blockedIPs.Contains(requestIP);
    }



    private string GetUserName()
    {
        if (Context.User != null && Context.User.Identity != null && Context.User.Identity.Name != null)
        {
            return Context.User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        }
        return "";
    }



    private async Task JoinRoom(string json)
    {
        RoomJoin join = JsonSerializer.Deserialize<RoomJoin>(json) ?? new RoomJoin();
       
        string userName = GetUserName();
        var rooms = Room.rooms;
        var room = rooms.FirstOrDefault(x => (x.name ?? "") == (join.RoomName ?? ""));
        if (room == null)
        {
            if (String.IsNullOrEmpty(join.RoomName))
            {
                // Глобальный чат
            }
            else
            {
                await Clients.Caller.SendAsync("system", "комната не найдена");
                return;    
            }
            
        }
        // --- НОВАЯ ЛОГИКА ОЧИСТКИ СТАРЫХ ГРУПП ---
        // Выкидываем текущее соединение (ConnectionId) из ВСЕХ существующих групп SignalR
        foreach (var existingRoom in rooms)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, existingRoom.name);
            existingRoom.Members = existingRoom.Members.Where(x => x != userName).ToList();
        }
        // ----------------------------------------

        if (room!=null && !string.IsNullOrEmpty(userName))
        {
            room.Members.Add(userName);
        }

        if (room==null && String.IsNullOrEmpty(room?.name))
        {
            // Глобальный чат
        }
        else
        {
            // Добавляем ТОЛЬКО в одну текущую группу
            await Groups.AddToGroupAsync(Context.ConnectionId, join.RoomName);
        }

        
        string membersJson = JsonSerializer.Serialize( rooms,  new JsonSerializerOptions(){WriteIndented=true});
        await File.WriteAllTextAsync("Rooms.json", membersJson);
        
        var messages = DbRecord.Records.Where(a => (a.group ?? "") == (room?.name ?? "")).ToList();;
        
        await Clients.Caller.SendAsync("I joined room", JsonSerializer.Serialize(messages));
    }
    
    private async Task CreateRoom(string json)
    {
        RoomJoin join = JsonSerializer.Deserialize<RoomJoin>(json) ?? new RoomJoin();
        if (join.RoomName == "")
        {
            await Clients.Caller.SendAsync("system", "Нужно название комнаты");
            return;
        }
       
        string userName = GetUserName();
        var rooms = Room.rooms;
        var room = rooms.FirstOrDefault(x => x.name == join.RoomName);
        if (room != null)
        {
            await Clients.Caller.SendAsync("system", "комната уже есть");
            return;
        }
        room = new Room(){name = join.RoomName, Members = new List<string>()};
        await Clients.Group(join.RoomName).SendAsync("Room created", room.name);
        rooms.Add(room);
    
        room.Members.Add(userName);
        await Groups.AddToGroupAsync(Context.ConnectionId, join.RoomName);
        
        string membersJson = JsonSerializer.Serialize( rooms,  new JsonSerializerOptions(){WriteIndented=true});
        await File.WriteAllTextAsync("Rooms.json", membersJson, Encoding.UTF8);
        
        await Clients.All.SendAsync("Room created", room.name);
    }

}


