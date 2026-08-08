using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Hospital.ClinicServices.WebApi.Hubs;

public class QueueHub : Hub
{
    public async Task JoinClinicQueueGroup(string clinicId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Clinic_{clinicId}");
    }

    public async Task LeaveClinicQueueGroup(int scheduleId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Clinic_{scheduleId}");
    }
}
