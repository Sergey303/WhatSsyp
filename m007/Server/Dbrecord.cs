using System.Text.Json;

public struct DbRecord
{
    public string user { get; set; }
    public bool? voice { get; set; }
    public string group { get; set; }
    public string message { get; set; } // текст или HTML-текст
    public string dt { get; set; } // DateTime
    public string filePath { get; set; } //file path

    public static List<DbRecord> Records => JsonSerializer.Deserialize<List<DbRecord>>(FileExtensions.ReadAllTextSafe("DataBase.json", "[]"));
}