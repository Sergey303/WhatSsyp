using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Collections.Generic;

public class ChatMessage {
    public string room { get; set; } = "general";
    public string name { get; set; } = "";
    public string text { get; set; } = "";
    public string time { get; set; } = "";
}

public class LoginMessage {
    public string username { get; set; } = "";
    public string login { get; set; } = "";
    public string password { get; set; } = "";
    
}

public class ChatHub : Hub
{
    private static Dictionary<string, string> userList = new Dictionary<string, string>()
    {
        ["ZOVchik"] = "12345678",
    };

    private static Dictionary<string, string> usernameList = new Dictionary<string, string>()
    {
        ["ZOVchik"] = "WGNR",
    };


    private static Dictionary<string, string> connectionList = new Dictionary<string, string>(){};

    public Task Send(string eventName, string text)
    {
        if (eventName == "register"){
            LoginMessage reg = JsonSerializer.Deserialize<LoginMessage>(text);
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

        if (eventName == "login"){
            LoginMessage login = JsonSerializer.Deserialize<LoginMessage>(text);
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
        
        ChatMessage message = JsonSerializer.Deserialize<ChatMessage>(text);
        string realName = connectionList[Context.ConnectionId];
        message.name = usernameList[realName];
        if (eventName == "joinRoom"){
            try {
            Groups.AddToGroupAsync(Context.ConnectionId, message.room).Wait();
            ChatMessage messagect = new ChatMessage();
            messagect.name = "SoZVon System";
            messagect.room = message.room;
            messagect.text = message.name + " Вошел в " + message.room;
            messagect.time = DateTime.Now.ToLongTimeString();
            string messagec = JsonSerializer.Serialize<ChatMessage>(messagect);
            return Clients.Group(text).SendAsync("chat", messagec);
            } catch (Exception ex) {
                Console.WriteLine($"Failed again: {ex.Message}");
            }
        }
        if (eventName == "chat"){
            if (!connectionList.ContainsKey(Context.ConnectionId)){
                return Clients.Caller.SendAsync("loginResult", "needLogin");
            }
            if (message == null) {
                message = new ChatMessage();
            }
            string textMessage = JsonSerializer.Serialize<ChatMessage>(message);
            return Clients.Group(message.room).SendAsync("chat", textMessage);
        }
        if (eventName == "newRoom"){
            Rooms.rooms.Add(message.room);
            Rooms.usersByRoom[message.room] = new[] {message.name};
            return Clients.Group(message.room).SendAsync("chat", text);
        }
        return Clients.Group(message.room).SendAsync("chat", text);
    }
}
