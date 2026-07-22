using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Text.Json;



public class ScheduleHub : Hub
{
    public async Task Send(string eventName, string text)
    {
        Console.WriteLine("[" + eventName + "]" + text);
        List<MyTask> tasks = await MyTask.GetListOfTaskAsync();
        if (eventName == "elementOfTable")
        {
            MyTask task = JsonSerializer.Deserialize<MyTask>(text);
            tasks.Add(task);
        }
        else if (eventName == "elementOfTable1")
        {
            int.TryParse(text, out var textInt);
            tasks.RemoveAt(textInt);
        }
        // tasks.Add(task);
        // tasks.Sort((x, y) => x.Time.CompareTo(y.Time));
        tasks.Sort((x, y) =>
        {
            TimeSpan t1 = TimeSpan.Parse(x.Time);
            TimeSpan t2 = TimeSpan.Parse(y.Time);
            return t1.CompareTo(t2);
        });
        await Clients.All.SendAsync(eventName, await MyTask.SaveListOfTaskAsync(tasks));
    }
    
}