using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Collections.Generic;

// Player player = new Player();
// player.Name = "";
// player.Score = 5;

// string json = JsonSerializer.Serialize(player);
// Console.WriteLine(json);

// Player restoredPlayer = JsonSerializer
//     .Deserialize<Player>(json)
//     ?? new Player();
// Console.WriteLine(restoredPlayer.Name);
List<Room> rooms = new List<Room>();

Room general = new Room();
general.Name = "Общий";
rooms.Add(general);

Room games = new Room();
general.Name = "Игры";
rooms.Add(games);

Random random = new Random();


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

var app = builder.Build();

app.MapGet("/api/rooms", () => rooms);

app.MapPost("/api/rooms", (Room room) =>
{
    rooms.Add(room);

    return Results.Ok();
});

app.MapGet("/api/dice", () => random.Next(1, 7));
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<ChatHub>("/chatHub");
string str = File.ReadAllText(@"wwwroot\index.html");
app.MapGet("/", () => Results.Content(str, "text/html"));


app.Run("http://0.0.0.0:5000");

