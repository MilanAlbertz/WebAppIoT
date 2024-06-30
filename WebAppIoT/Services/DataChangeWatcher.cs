using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WebAppIoT.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace WebAppIoT.Services
{
    public class DataChangeWatcher : BackgroundService
    {
        private readonly IHubContext<DataHub> _hubContext;
        private readonly IServiceProvider _services;
        private DateTime _lastCheckedTimeStamp;

        public DataChangeWatcher(IHubContext<DataHub> hubContext, IServiceProvider services)
        {
            _hubContext = hubContext;
            _services = services;
            _lastCheckedTimeStamp = DateTime.MinValue; // Initialize with MinValue or retrieve from persistent store
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        // Retrieve the latest timestamp from the database
                        var latestTimeStamp = await dbContext.Data
                            .OrderByDescending(d => d.TimeStamp)
                            .Select(d => d.TimeStamp)
                            .FirstOrDefaultAsync();

                        if (latestTimeStamp != _lastCheckedTimeStamp)
                        {
                            // New timestamp detected, notify clients via SignalR
                            await _hubContext.Clients.All.SendAsync("ReceiveDataUpdate");

                            // Update the last checked timestamp
                            _lastCheckedTimeStamp = latestTimeStamp;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions, e.g., logging or graceful handling
                    Console.WriteLine($"Error in DataChangeWatcher: {ex.Message}");
                }

                // Delay before the next check
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); // Adjust delay as needed
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
