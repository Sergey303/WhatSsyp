using System.Text.Json;


public class AccountProcessor
{
    public static string filePath = "accounts.json";
    public List<Account> LoadAccounts()
    {
        try
        {
            string jsonStr = FileExtensions.ReadAllTextSafe(filePath, "[]");
            return JsonSerializer.Deserialize<List<Account>>(jsonStr);
        }
        catch
        {
            return new List<Account>();
        }
    }
}

public class Account
{
    public string name { get; set; } = string.Empty;
    public string login { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}
