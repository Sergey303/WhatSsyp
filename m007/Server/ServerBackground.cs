using Microsoft.AspNetCore.SignalR; 
using System.Text.Json; 
using System.Collections.Generic;

public class MyBackGroundService : BackgroundService 
{ 
    private readonly IHubContext<ScheduleHub> _hubContext; 

    public MyBackGroundService(IHubContext<ScheduleHub> hubContext) 
    { 
        _hubContext = hubContext; 
    } 

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
    { 
        // Relinquish control to allow Kestrel to bind to its ports immediately
        await Task.Yield(); 

        while (!stoppingToken.IsCancellationRequested) 
        { 
            try
            {
                DateTime date = DateTime.Now; 
                int currentHour = date.Hour; 
                int currentMinute = date.Minute; 

                // Use the async version of your task loader
                List<MyTask> tasksForAlert = await MyTask.GetListOfTaskAsync(); 
                
                List<MyTask> tasksForAlertNow = new List<MyTask>(); 
                List<MyTask> taskOverdue = new List<MyTask>(); 

                foreach (MyTask taskForAlert in tasksForAlert) 
                { 
                    string time = taskForAlert.Time; 
                    if (string.IsNullOrWhiteSpace(time) || !time.Contains(':')) continue;

                    char[] separators = [':']; 
                    string[] hoursAndMinutes = time.Split(separators); 
                    
                    int.TryParse(hoursAndMinutes[0], out int hours); 
                    int.TryParse(hoursAndMinutes[1], out int minutes); 

                    TimeSpan currentTime = new TimeSpan(currentHour, currentMinute, 0); 
                    TimeSpan userTime = new TimeSpan(hours, minutes, 0); 
                    TimeSpan difference = userTime.Subtract(currentTime); 

                    if (difference.TotalMinutes <= 5) 
                    { 
                        if (difference.TotalMinutes < 0) 
                        { 
                            taskOverdue.Add(taskForAlert); 
                        } 
                        else 
                        { 
                            tasksForAlertNow.Add(taskForAlert); 
                        } 
                    } 
                } 

                if (tasksForAlertNow.Count != 0) 
                { 
                    string alert = JsonSerializer.Serialize(tasksForAlertNow); 
                    await _hubContext.Clients.All.SendAsync("timer1", alert, stoppingToken); 
                } 

                if (taskOverdue.Count != 0) 
                { 
                    string alertOverdue = JsonSerializer.Serialize(taskOverdue); 
                    await _hubContext.Clients.All.SendAsync("timer2", alertOverdue, stoppingToken); 
                }
            }
            catch (Exception ex)
            {
                // Prevent background processing errors from crashing the entire app host
                Console.WriteLine($"Error processing schedule tasks: {ex.Message}");
            }

            // Sleep for 30 seconds before repeating
            await Task.Delay(30000, stoppingToken); 
        } 
    } 
} 

class MyTask 
{ 
    public string Time { get; set; } = ""; 
    public string Task { get; set; } = ""; 

    // Refactored to be asynchronous and handle error states safely
    public static async Task<List<MyTask>> GetListOfTaskAsync() 
    { 
        string path = Path.Combine("wwwroot", "YourSchedule", "base.json");

        if (!File.Exists(path))
        {
            return new List<MyTask>();
        }

        try
        {
            string tasks = await FileExtensions.ReadAllTextSafeAsync(path, "[]"); 
            return JsonSerializer.Deserialize<List<MyTask>>(tasks) ?? new List<MyTask>(); 
        }
        catch
        {
            return new List<MyTask>();
        }
    } 

    public static async Task<string> SaveListOfTaskAsync(List<MyTask> tasks) 
    { 
        string path = Path.Combine("wwwroot", "YourSchedule", "base.json");
        var options = new JsonSerializerOptions { WriteIndented = true }; 
        string json = JsonSerializer.Serialize(tasks, options); 
        
        // Ensure directory exists before saving
        string? directory = Path.GetDirectoryName(path);
        if (directory != null) Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(path, json, System.Text.Encoding.UTF8); 
        return json; 
    } 
}
