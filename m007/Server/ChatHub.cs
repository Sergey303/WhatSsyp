using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Security.Claims;
using System.Text;

public class ChatMessage {
    public string group { get; set; } = "general";
    public string user { get; set; } = "";
    public string message { get; set; } = "";
    public string dt { get; set; } = "";
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
    List<Room> roomMembers=JsonSerializer.Deserialize<List<Room>>(File.ReadAllText("RoomsAlexander.json"));
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
                Rooms.rooms.Add(message.group);
                Rooms.usersByRoom[message.group] = new[] {message.user};
                Rooms.messagesByRoom[message.group] = new List<ChatMessage> {message};
                return Clients.Group(message.group).SendAsync("chat", jtext);
            }

            return Clients.Group(message.group).SendAsync("chat", jtext);
        }
        else if (eventName == "joinRoom") {
            return joinRoom(jtext);
        } else if (eventName == "chat") {
            Message message = JsonSerializer.Deserialize<Message>(jtext);
            string messageText = message.text;
            string room = message.room;
            return SendChat(messageText, room);
        }
        
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
            jtext = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<jsonMsg>(jtext).name = name);
            if (eventName == "MLChat")
            {
                return OldSendChat(jtext);
            }
            return Clients.All.SendAsync("chat", jtext);
        }


        var jmes = JsonSerializer.Deserialize<DbRecord>(jtext);
        var fileInString = File.ReadAllText("DataBase.json");
        var db = JsonSerializer.Deserialize<List<DbRecord>>(fileInString);
        //message = jtext, dt = DateTime.Now.ToString("d")
        
        db.Add(new DbRecord() {message = jmes.message, user = jmes.user, group = jmes.group, dt = DateTime.Now.ToString()});
        var newText = JsonSerializer.Serialize<List<DbRecord>>(db, new JsonSerializerOptions(){WriteIndented=true});
        File.WriteAllText("DataBase.json", newText);
        return Clients.All.SendAsync("chat", jtext);
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
        string name = "";
        if (Context?.User?.Identity?.Name != null)
        {
            name = Context.User.Identity.Name;
        }

        if (name == "")
        {
            return Clients.Caller.SendAsync("system", "Сначала войди");
        }

        return Clients.All.SendAsync("chat", JsonSerializer.Serialize(
                JsonSerializer.Deserialize<jsonMsg>(text).name = name));
    }
    public bool isIpBlocked()
    {
        string jsnBlockedIPs = File.ReadAllText("wwwroot/BlockedIPs.json", Encoding.UTF8);
        List<string>? blockedIPs = JsonSerializer.Deserialize<List<string>>(jsnBlockedIPs);
        string? requestIP = this.Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
        return blockedIPs.Contains(requestIP);
    }
    private Task joinRoom(string json)
    {
        RoomJoin join = JsonSerializer.Deserialize<RoomJoin>(json) ?? new RoomJoin();
        if (join.RoomName == "" || join.UserName == "")
        {
            return Clients.Caller.SendAsync("system", "нужное имя и название комнаты");
        }
        Groups.AddToGroupAsync(Context.ConnectionId, join.RoomName).Wait();
        if (roomMembers.FirstOrDefault(x => join.RoomName == x.name) != null)
        {
            //List<string> members = roomMembers[join.RoomName];
            List<string> members = roomMembers.FirstOrDefault(x => join.RoomName == x.name).Members;
            if (members.Contains(join.UserName) == false)
            {
                members.Add(join.UserName);
            }
            roomMembers.FirstOrDefault(x => join.RoomName == x.name).Members = members;
            string output = JsonSerializer.Serialize(roomMembers);
            File.WriteAllText("RoomsAlexander.json", output);
            string membersJson = JsonSerializer.Serialize(members);
            return Clients.Group(join.RoomName).SendAsync("roomMembers", membersJson);
        }
        else
        {
            return Task.CompletedTask;
        }
        
    }
}

public struct DbRecord
{
    public string user {get; set;}
    public string group {get; set;}
    public string message {get; set;} // текст или HTML-текст
    public string dt{get; set;} // DateTime
}
