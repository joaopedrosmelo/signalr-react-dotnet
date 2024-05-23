using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace APISignalR
{
    public class ReportHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task NotifyReportReady(string reportUrl)
        {
            await Clients.All.SendAsync("ReportReady", reportUrl);
        }
    }
}
