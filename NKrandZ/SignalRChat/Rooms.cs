public static class Rooms{
    public static List<string> rooms { get; set; } = new List<string> { "General", "Games", "School" };
    public static Dictionary<string, List<string>> usersByRoom = new Dictionary<string, List<string>>();
}