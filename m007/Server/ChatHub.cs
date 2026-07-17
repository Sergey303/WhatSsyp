using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Security.Claims;

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

public class ChatHub : Hub {

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

        var jmes = JsonSerializer.Deserialize<DbRecord>(jtext);
        var fileInString = File.ReadAllText("DataBase.json");
        var db = JsonSerializer.Deserialize<List<DbRecord>>(fileInString);
        //message = jtext, dt = DateTime.Now.ToString("d")
        
        db.Add(new DbRecord() {message = jmes.message, user = jmes.user, group = jmes.group, dt = DateTime.Now.ToString()});
        var newText = JsonSerializer.Serialize<List<DbRecord>>(db, new JsonSerializerOptions(){WriteIndented=true});
        File.WriteAllText("DataBase.json", newText);
        return Clients.All.SendAsync("chat", jtext);
    }
}

public struct DbRecord
{
    public string user {get; set;}
    public string group {get; set;}
    public string message {get; set;} // текст или HTML-текст
    public string dt{get; set;} // DateTime
}
