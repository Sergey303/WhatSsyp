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
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) // 
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime date = DateTime.Now;
            int currentHour = date.Hour;
            int currentMinute = date.Minute;
            List<MyTask> tasksForAlert = MyTask.GetListOfTask();
            List<MyTask> tasksForAlertNow = new List<MyTask>();
            List<MyTask> taskOverdue = new List<MyTask>();
            foreach (MyTask taskForAlert in tasksForAlert)
            {
                string time = taskForAlert.Time;
                char[] seperators = [':'];
                string[] hoursAndMinutes = time.Split(seperators);
                int.TryParse(hoursAndMinutes[0], out int hours);
                int.TryParse(hoursAndMinutes[1], out int minutes);
                TimeSpan currentTime = new TimeSpan(currentHour, currentMinute, 0);
                TimeSpan userTime = new TimeSpan(hours, minutes, 0);
                // TimeSpan fiveMinutes = new TimeSpan(0, 5, 0);
                TimeSpan difference = userTime.Subtract(currentTime);
                if (difference.TotalMinutes <= 5)
                {
                    if (difference.TotalMinutes < 0)
                    {
                        taskOverdue.Add(taskForAlert);
                        // await _hubContext.Clients.All.SendAsync("timer", "");
                    }
                    else
                    {
                        tasksForAlertNow.Add(taskForAlert);
                    }
                    // await _hubContext.Clients.All.SendAsync("timer", "");


                }

            }
            if (tasksForAlertNow.Count != 0)
            {
                string alert = JsonSerializer.Serialize(tasksForAlertNow);
                await _hubContext.Clients.All.SendAsync("timer1", alert);
            }
            if (taskOverdue.Count != 0)
            {   
                string alertOverdue = JsonSerializer.Serialize(taskOverdue);
                await _hubContext.Clients.All.SendAsync("timer2", alertOverdue);
            }
            await Task.Delay(30000, stoppingToken);
        }
        
        
    }
}
class MyTask
    {
        public string Time
        {
            get;
            set;
        } = "";

        public string Task
        {
            get;
            set;
        } = "";

        public static List<MyTask> GetListOfTask()
        {
            string tasks = File.ReadAllText(@"wwwroot\YourSchedule\base.json");
            return JsonSerializer.Deserialize<List<MyTask>>(tasks);

        }
        public static void SaveListOfTask(List<MyTask> tasks) {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(tasks, options);
            File.WriteAllText(@"wwwroot\YourSchedule\base.json", json, System.Text.Encoding.UTF8);
        }
    }


