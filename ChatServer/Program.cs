using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace ChatServer
{
    public class Program
    {
        private static NotificationHubController notificationController;

        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var hubContext = host.Services.GetService(typeof(IHubContext<NotificationHub>));
            
            notificationController = new NotificationHubController((IHubContext<NotificationHub>) hubContext);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class NotificationHubController : Controller
    {
        public IHubContext<NotificationHub> _hubContext { get; private set; }

        public NotificationHubController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendMessage(string message)
        {
            await _hubContext.Clients.All.SendAsync("Send", message);
        }
    }
}
