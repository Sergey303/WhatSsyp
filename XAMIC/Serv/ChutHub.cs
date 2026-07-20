using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Primitives;
using System.Data;
public class ChatHub : Hub
{
    //private static Dictionary<string, List <string>> roomMembers = new Dictionary<string, List<string>>();
    public Task Send(string eventName, string text)
    {

        if (eventName == "chat")
        {
            Message message = JsonSerializer.Deserialize<Message>(text);
            string messageText = message.text;
            string room = message.room;
            return SendChat(messageText, room);
        }
        if (eventName == "joinRoom")
        {
            RoomJoin join = JsonSerializer.Deserialize<RoomJoin>(text);
            return joinRoom(JsonSerializer.Serialize(new List<string> { join.RoomName, Context.User.FindFirst("UserName").Value }));
        }
        if (eventName == "connected")
        {
            OnConnectedAsync();
        }
        return Clients.All.SendAsync(eventName, text);
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
            List<Message> messages = JsonSerializer.Deserialize<List <Message>>(File.ReadAllText("DataMessages.json"));
            messages.Add(new Message {name=name, text=text, room=room});
            string convert = JsonSerializer.Serialize(messages, new JsonSerializerOptions{WriteIndented=true});
            File.WriteAllText("DataMessages.json", convert, Encoding.UTF8);
            return Clients.Group(room).SendAsync("chat", JsonSerializer.Serialize<Message>(new Message {name=name, text=text, room=room}));
        }
        return Clients.Caller.SendAsync("system", "Сначала войди");
    }
    private Task joinRoom(string json)
    {
        List<Room> roomMembers = JsonSerializer.Deserialize<List<Room>>(File.ReadAllText("Rooms.json"));

        List<string> convert = JsonSerializer.Deserialize<List<string>>(json);
        string RoomName = convert[0];
        string UserName = convert[1];
        if (RoomName == "" || UserName == "")
        {
            return Clients.Caller.SendAsync("system", "нужное имя и название комнаты");
        }
        Groups.AddToGroupAsync(Context.ConnectionId, RoomName).Wait();
        Room room = roomMembers.FirstOrDefault(x => RoomName == x.name);
        if (room != null)
        {
            //List<string> members = roomMembers[join.RoomName];
            if (room.Members.Contains(UserName) == false)
            {
                room.Members.Add(UserName);
            }
            string output = JsonSerializer.Serialize(roomMembers);
            File.WriteAllText("Rooms.json", output, Encoding.UTF8);
            string membersJson = JsonSerializer.Serialize(room.Members);
            return Clients.Group(RoomName).SendAsync("roomMembers", membersJson);
        }
        else
        {
            return Task.CompletedTask;
        }
    }
    public override async Task OnConnectedAsync()
    {
        List<Room> roomMembers = JsonSerializer.Deserialize<List<Room>>(File.ReadAllText("Rooms.json"));
        string connectionId = Context.ConnectionId;
        foreach (Room item in roomMembers)
        {
            if (item.Members.Contains(Context.User.FindFirst("UserName").Value))
            {
                await Groups.AddToGroupAsync(connectionId, item.name);
            }
        }
        await base.OnConnectedAsync();
    }

}

