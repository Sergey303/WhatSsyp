public static class FileExtensions
{
    public static string ReadAllTextSafe(string path, string defaultValue = "")
    {
        return File.Exists(path) ? File.ReadAllText(path) : defaultValue;
    }
    public static async Task<string> ReadAllTextSafeAsync(string path, string defaultValue = "")
    {
        if (!File.Exists(path))
        {
            return defaultValue; // Или дефолтный текст
        }
    
        return await File.ReadAllTextAsync(path);
    }
}

