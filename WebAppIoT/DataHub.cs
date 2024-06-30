using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WebAppIoT.Hubs
{
    public class DataHub : Hub
    {
        // No need to change this part if already correctly set up as previously shown
        public async Task NotifyDataChanged()
        {
            await Clients.All.SendAsync("ReceiveDataUpdate");
        }
    }
}
