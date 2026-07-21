public static class Rooms{
    public static List<string> rooms { get; set; } = new List<string> { "General", "Games", "School" };
    public static Dictionary<string, string[]> usersByRoom = new Dictionary<string, string[]>();
    public static Dictionary<string, List<ChatMessage>> messagesByRoom = new Dictionary<string, List<ChatMessage>>();
}