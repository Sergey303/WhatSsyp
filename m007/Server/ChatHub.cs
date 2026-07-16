using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Security.Claims;

public class ChatHub : Hub {
    
    public Task Send(string eventName, string jtext) {
        var jmes = JsonSerializer.Deserialize<DbRecord>(jtext);
        var fileInString = File.ReadAllText("DataBase.json");
        var db = JsonSerializer.Deserialize<List<DbRecord>>(fileInString);
        //message = jtext, dt = DateTime.Now.ToString("d")
        
        db.Add(new DbRecord() {message = jmes.message, user = jmes.user, group = jmes.group, dt = DateTime.Now.ToString("d")});
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
