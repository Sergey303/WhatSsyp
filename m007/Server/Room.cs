using System.Text.Json;

public class Room
{
    public string name
    {
        get;
        set;
    } = "";
    public List<string> Members
    {
        get;
        set;
    } = [];
    public static List<Room> rooms => JsonSerializer.Deserialize<List<Room>>(FileExtensions.ReadAllTextSafe("Rooms.json", "[]"));
    
}