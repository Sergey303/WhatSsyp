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
        return Clients.All.SendAsync(eventName, text);
    }
}