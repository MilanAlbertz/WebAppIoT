using Microsoft.Extensions.Hosting;
using System;
using System.Data;
using System.Data.SqlClient;
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
        private long _lastProcessedVersion;

        public DataChangeWatcher(IHubContext<DataHub> hubContext, IServiceProvider services)
        {
            _hubContext = hubContext;
            _services = services;
            _lastProcessedVersion = 0; // Initialize with 0 or retrieve from persistent store
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken); // Adjust delay as needed

                using (var scope = _services.CreateScope())
                {
                    var connectionString = scope.ServiceProvider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");

                    // Construct query to get latest change version
                    string query = @"
                        SELECT CHANGE_TRACKING_CURRENT_VERSION() AS ChangeVersion;
                    ";

                    long currentVersion = 0;

                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand(query, connection))
                        {
                            await connection.OpenAsync(stoppingToken);
                            var result = await command.ExecuteScalarAsync(stoppingToken);
                            if (result != DBNull.Value)
                            {
                                currentVersion = (long)result;
                            }
                        }
                    }

                    if (currentVersion > _lastProcessedVersion)
                    {
                        // New changes detected, notify clients via SignalR
                        await _hubContext.Clients.All.SendAsync("ReceiveDataUpdate");
                        _lastProcessedVersion = currentVersion;
                    }
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
