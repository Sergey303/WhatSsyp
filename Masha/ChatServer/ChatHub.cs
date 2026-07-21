using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Text.Json;



public class ChatHub : Hub
{
    public Task Send(string eventName, string text)
    {
        Console.WriteLine("[" + eventName + "]" + text);
        MyTask task = JsonSerializer.Deserialize<MyTask>(text);
        List<MyTask> tasks = MyTask.GetListOfTask();
        if (eventName == "elementOfTable")
        {
            tasks.Add(task);
        }
        else if (eventName == "elementOfTable1")
        {
            tasks.RemoveAll(MyTask1 => MyTask1.Time == task.Time && MyTask1.Task == task.Task);
        }
        // tasks.Add(task);
        tasks.Sort((x, y) => x.Time.CompareTo(y.Time));
        MyTask.SaveListOfTask(tasks);
        return Clients.All.SendAsync(eventName, text);
    }
    
}