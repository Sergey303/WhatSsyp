using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Security.Claims;
using System.Text;
public class ChatHub : Hub
{
    List<Room> roomMembers=JsonSerializer.Deserialize<List<Room>>(File.ReadAllText("Rooms.json"));
    //private static Dictionary<string, List <string>> roomMembers = new Dictionary<string, List<string>>();
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
            File.WriteAllText("Rooms.json", output, Encoding.UTF8);
            string membersJson = JsonSerializer.Serialize(members);
            return Clients.Group(join.RoomName).SendAsync("roomMembers", membersJson);
        }
        else
        {
            return Task.CompletedTask;
        }
        
    }
}

