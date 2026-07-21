public struct DbRecord
{
    public string user { get; set; }
    public string group { get; set; }
    public string message { get; set; } // текст или HTML-текст
    public string dt { get; set; } // DateTime
    public string filePath { get; set; } //file path
}