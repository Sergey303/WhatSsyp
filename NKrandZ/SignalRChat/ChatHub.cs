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
    private static Dictionary<string, string[]> userList = new Dictionary<string, string[]>()
    {
        ["ZOVchik"] = new string[] {"Zovchik-WGNR", "12345678"}
    };

    private static Dictionary<string, string[]> connectionList = new Dictionary<string, string[]>(){};

    public Task Send(string eventName, string text)
    {
        if (eventName == "login"){
            LoginMessage login = JsonSerializer.Deserialize<LoginMessage>(text);
            if (login == null){
                login = new LoginMessage();
            }
            bool hasUser = userList.ContainsValue(new string[] {login.login, login.password});
            if (hasUser){
                connectionList[ConnectionId] = login.login;
                return Clients.Caller.SendAsync("loginResult", "Login Complete");
            }
            return Clients.Caller.SendAsync("loginResult", "error");
        }
        
        ChatMessage message = JsonSerializer.Deserialize<ChatMessage>(text);
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
            string realName = connectionNames[Context.ConnectionId];
            message.name = realName;
            string textMessage = JsonSerializer.Serialize<ChatMessage>(message);
            return Clients.Group(message.room).SendAsync("chat", text);
        }
        if (eventName == "newRoom"){
            Rooms.rooms.Add(message.room);
            Rooms.usersByRoom[message.room] = new[] {message.name};
            return Clients.Group(message.room).SendAsync("chat", text);
        }
        return Clients.Group(message.room).SendAsync("chat", text);
    }
}
