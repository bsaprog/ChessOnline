using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ChatServer
{
    public class NotificationHub : Hub
    {
        public Task SendMessage(string message)
        {
            return Clients.All.SendAsync("Send", message);
        }
    }
}
