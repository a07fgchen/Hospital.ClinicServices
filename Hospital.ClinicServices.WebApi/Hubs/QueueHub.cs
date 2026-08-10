using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Hospital.ClinicServices.WebApi.Hubs;

public class QueueHub : Hub
{
    public async Task JoinClinicQueueGroup(int scheduleId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Clinic_{scheduleId}");
    }

    public async Task LeaveClinicQueueGroup(int scheduleId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Clinic_{scheduleId}");
    }
}
