using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Security.Claims;
using System.Text;
using System.Dynamic;

public class ChatMessage {
     public string group { get; set; } = "general";
     public string user { get; set; } = "";
     public string message { get; set; } = "";
     public string dt { get; set; } = "";
}

public class ChatMessageAndrey
{
    public string sender { get; set; } = "";
    public string text { get; set; } = "";
    public string time { get; set; } = "";
    public string type { get; set; } = "text";
}

public class LoginMessage {
    public string username { get; set; } = "";
    public string login { get; set; } = "";
    public string password { get; set; } = "";
    
}

class jsonMsg
{
    public string name { get; set; } = "";
    public string text { get; set; } = "";
    public string filePath { get; set; } = "";
    public string date { get; set; } = "";
    public string room { get; set; } = "";
}

public class ChatHub : Hub {
    //List<Room> roomMembers=JsonSerializer.Deserialize<List<Room>>(File.ReadAllText("RoomsAlexander.json"));
    private static Dictionary<string, List<string>> roomMembersAndrey = new Dictionary<string, List<string>>();
    private static Dictionary<string, List<ChatMessageAndrey>> roomMessages = new Dictionary<string, List<ChatMessageAndrey>>();
    private static Dictionary<string, string> connectionRooms = new Dictionary<string, string>();
    private static string messagesFilePath = "wwwroot/messages.json";
    private static Dictionary<string, List<string>> roomMembers = new Dictionary<string, List<string>>();
    private static Dictionary<string, string> userList = new Dictionary<string, string>()
    {
        ["ZOVchik"] = "12345678",
    };

    private static Dictionary<string, string> usernameList = new Dictionary<string, string>()
    {
        ["ZOVchik"] = "WGNR",
    };

    private static Dictionary<string, string> connectionList = new Dictionary<string, string>(){};

    
    public Task Send(string eventName, string jtext) {
        
        if (eventName.StartsWith("SoZVoN")){

            if (eventName == "SoZVoNregister"){
                LoginMessage reg = JsonSerializer.Deserialize<LoginMessage>(jtext);
                if (reg == null){
                    reg = new LoginMessage();
                }
                if (userList.ContainsKey(reg.login)){
                    return Clients.Caller.SendAsync("loginResult", reg.login + " error " + reg.password);
                }
                userList[reg.login] = reg.password;
                usernameList[reg.login] = reg.username;
                connectionList[Context.ConnectionId] = reg.login;
                return Clients.Caller.SendAsync("loginResult", "Login Complete");
            }

            if (eventName == "SoZVoNlogin"){
                LoginMessage login = JsonSerializer.Deserialize<LoginMessage>(jtext);
                if (login == null){
                    login = new LoginMessage();
                }
                bool hasUser = userList.ContainsKey(login.login);
                if (hasUser && userList[login.login] == login.password){
                    connectionList[Context.ConnectionId] = login.login;
                    return Clients.Caller.SendAsync("loginResult", "Login Complete");
                }
                return Clients.Caller.SendAsync("loginResult", login.login + " error " + login.password);
            }
        

            ChatMessage message = JsonSerializer.Deserialize<ChatMessage>(jtext);
            string realName = connectionList[Context.ConnectionId];
            message.user = usernameList[realName];
            if (eventName == "SoZVoNjoinRoom"){
                try {
                    if (!(Rooms.usersByRoom[message.group].Contains(message.user))){
                        string[] newNumbers = new string[Rooms.usersByRoom[message.group].Length + 1];
                        Array.Copy(Rooms.usersByRoom[message.group], newNumbers, Rooms.usersByRoom[message.group].Length);
                        newNumbers[newNumbers.Length - 1] = message.user;
                        Rooms.usersByRoom[message.group] = newNumbers;
                        ChatMessage messagect = new ChatMessage();
                        messagect.user = "SoZVon System";
                        messagect.group = message.group;
                        messagect.message = message.user + " Вошел в " + message.group;
                        messagect.dt = DateTime.Now.ToLongTimeString();
                        string messagec = JsonSerializer.Serialize<ChatMessage>(messagect);
                        return Clients.Group(jtext).SendAsync("chat", messagec);
                    }
                    Clients.Caller.SendAsync("historyFirst", "first").Wait();
                    Groups.AddToGroupAsync(Context.ConnectionId, message.group).Wait();
                    foreach (ChatMessage i in Rooms.messagesByRoom[message.group]){
                        string textMessage = JsonSerializer.Serialize<ChatMessage>(i);
                        Clients.Caller.SendAsync("messageHistory", textMessage).Wait();
                    }
                    return Clients.Caller.SendAsync("historyLast", "last");
                } catch (Exception ex) {
                    Console.WriteLine($"Failed again: {ex.Message}");
                }
            }

            if (eventName == "SoZVoNchat"){
                if (!connectionList.ContainsKey(Context.ConnectionId)){
                    return Clients.Caller.SendAsync("loginResult", "needLogin");
                }
                if (message == null) {
                    message = new ChatMessage();
                }
                Rooms.messagesByRoom[message.group].Add(message);
                string textMessage = JsonSerializer.Serialize<ChatMessage>(message);
                return Clients.Group(message.group).SendAsync("chat", textMessage);
            }

            if (eventName == "SoZVoNnewRoom"){
                if (Rooms.rooms.Contains(message.group)){return Clients.Group(message.group).SendAsync("chat", jtext);}
                Rooms.rooms.Add(message.group);
                Rooms.usersByRoom[message.group] = new[] {message.user};
                Rooms.messagesByRoom[message.group] = new List<ChatMessage> {message};
                return Clients.Group(message.group).SendAsync("chat", jtext);
            }

            return Clients.Group(message.group).SendAsync("chat", jtext);
        }
        // else 
        // if (eventName == "joinRoom") {
        //     return joinRoom(jtext);
        // } 
        // else 
        // if (eventName == "chat") {
        //     Message message = JsonSerializer.Deserialize<Message>(jtext);
        //     string messageText = message.text;
        //     string room = message.room;
        //     return SendChat(messageText, room);
        // }
        
        else if (eventName == "MLChat") {
            string? name = this.Context.GetHttpContext().User?.Identity?.Name?.ToString();
            if (name == null)
            {
                return Clients.Caller.SendAsync("429");
            }
            if (isIpBlocked())
            {
                return Clients.Caller.SendAsync("429");
            }
            if (eventName == "MLChat")
            {
                return OldSendChat(jtext);
            }
            return Clients.All.SendAsync("chat", jtext);
        } 
        else if (eventName == "chat")
        {
            string name = GetUserName();
            if (string.IsNullOrEmpty(name))
            {
                await Clients.Caller.SendAsync("system", "Сначала войди");
                return;
            }

           // SendChat(jtext);
             var jmes = JsonSerializer.Deserialize<DbRecord>(jtext);
            var fileInString = File.ReadAllText("DataBase.json");
            var db = JsonSerializer.Deserialize<List<DbRecord>>(fileInString);
            //message = jtext, dt = DateTime.Now.ToString("d")
            
            db.Add(new DbRecord() {message = jmes.message, user = name, group = jmes.group, dt = DateTime.Now.ToString()});
            var newText = JsonSerializer.Serialize<List<DbRecord>>(db, new JsonSerializerOptions(){WriteIndented=true});
            File.WriteAllText("DataBase.json", newText);
            return Clients.All.SendAsync("chat", jtext);
        }
        else if (eventName == "joinRoom")
        {
            joinRoom(jtext);
        }
        else if (eventName == "file")
        {
            SendFileMessage(jtext);
        }
        else if (eventName == "voice")
        {
            SendVoiceMessage(jtext);
        }


      
    }
    private Task SendChat(string text, string room)
    {
        string name = "";
        if (Context.User != null && Context.User.Identity != null && Context.User.Identity.Name != null)
        {
            //name = Context.User.Claims.FirstOrDefault((x)=>((x.Type == ClaimTypes.UserData)))?.Value;
            string userName = "UserName";
            name = Context.User.FindFirst(userName).Value;
            foreach (var item in Context.User.Claims)
            {
                Console.WriteLine(item.Value);
                Console.WriteLine(item.Type);
            }
        }
        
        if (name == "")
        {
            return Clients.Caller.SendAsync("system", "Сначала войди");
        }
        return Clients.Group(room).SendAsync("chat", room + ":" + name + ": " + text);
    }
    
    private Task OldSendChat(string text)
    {
        
        if (Context?.User?.Identity?.Name != null)
        {
            var jsonMsg_ = JsonSerializer.Deserialize<jsonMsg>(text);
            jsonMsg_.name = Context.User.Identity.Name;
            text = JsonSerializer.Serialize(jsonMsg_);
        }
        else
        {
            return Clients.Caller.SendAsync("system", "Сначала войди");
        }
        Console.WriteLine(text);
        return Clients.All.SendAsync("chat", text);
    }
    public bool isIpBlocked()
    {
        string jsnBlockedIPs = File.ReadAllText("wwwroot/BlockedIPs.json", Encoding.UTF8);
        List<string>? blockedIPs = JsonSerializer.Deserialize<List<string>>(jsnBlockedIPs);
        string? requestIP = this.Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
        return blockedIPs.Contains(requestIP);
    }
    // private Task joinRoom(string json)
    // {
    //     RoomJoin join = JsonSerializer.Deserialize<RoomJoin>(json) ?? new RoomJoin();
    //     if (join.RoomName == "" || join.UserName == "")
    //     {
    //         return Clients.Caller.SendAsync("system", "нужное имя и название комнаты");
    //     }
    //     Groups.AddToGroupAsync(Context.ConnectionId, join.RoomName).Wait();
    //     if (roomMembers.FirstOrDefault(x => join.RoomName == x.name) != null)
    //     {
    //         //List<string> members = roomMembers[join.RoomName];
    //         List<string> members = roomMembers.FirstOrDefault(x => join.RoomName == x.name).Members;
    //         if (members.Contains(join.UserName) == false)
    //         {
    //             members.Add(join.UserName);
    //         }
    //         roomMembers.FirstOrDefault(x => join.RoomName == x.name).Members = members;
    //         string output = JsonSerializer.Serialize(roomMembers);
    //         File.WriteAllText("RoomsAlexander.json", output);
    //         string membersJson = JsonSerializer.Serialize(members);
    //         return Clients.Group(join.RoomName).SendAsync("roomMembers", membersJson);
    //     }
    //     else
    //     {
    //         return Task.CompletedTask;
    //     }
        
    // }
    private async Task SendChat(string text)
    {
        string name = GetUserName();
        if (string.IsNullOrEmpty(name))
        {
            await Clients.Caller.SendAsync("system", "Сначала войди");
            return;
        }

        string roomName = GetRoomName();
        
        var message = new ChatMessageAndrey
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
        
        var message = new ChatMessageAndrey
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
        
        var message = new ChatMessageAndrey
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

    private void AddMessageToRoom(string roomName, ChatMessageAndrey message)
    {
        if (!roomMessages.ContainsKey(roomName))
        {
            roomMessages[roomName] = new List<ChatMessageAndrey>();
        }
        roomMessages[roomName].Add(message);
        SaveMessages();
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

    // public bool isIpBlocked(string name)
    // {
    //     string jsnBlockedIPs = File.ReadAllText("wwwroot/BlockedIPs.json", Encoding.UTF8);
    //     List<string>? blockedIPs = JsonSerializer.Deserialize<List<string>>(jsnBlockedIPs);
    //     string? requestIP = this.Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
    //     Console.WriteLine($"{name}.   IP: {requestIP}");
    //     return blockedIPs != null && blockedIPs.Contains(requestIP);
    // }
}


