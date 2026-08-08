using Hospital.ClinicServices.WebApi.Data;
using Hospital.ClinicServices.WebApi.Entities;
using Hospital.ClinicServices.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Hospital.ClinicServices.WebApi.Services.Interfaces;

public class CallingService : ICallingService
{
    private readonly ClinicDbContext _context;

    // 注入 SignalR 上下文，讓一般 Service 也能發送推播
    private readonly IHubContext<QueueHub> _hubContext;

    public CallingService(ClinicDbContext context, IHubContext<QueueHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<Schedule> CallingNextClinicAsync(int scheduleId)
    {
        var schedule = await _context.Schedules
            .Include(s => s.Doctor)
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

        if (schedule == null)
        {
            throw new Exception("找不到該門診排班。");
        }

        if (schedule.Status != 1)
        {
            throw new Exception("該門診目前非看診狀態。");
        }

        //檢查是否已經叫完所有掛號的人
        if (schedule.CurrentCallingNumber >= schedule.CurrentRegisterCount)
        {
            throw new Exception("目前已為排隊等候的病人。");
        }

        // 1. 叫號號碼遞增
        schedule.CurrentCallingNumber++;
        _context.Schedules.Update(schedule);
        await _context.SaveChangesAsync();

        // 2. 核心亮點：透過 SignalR 即時推播給「關注此診間」的所有用戶端
        // 傳送匿名物件，包含當前最新號碼、診間號碼與狀態
        await _hubContext.Clients.Group($"Clinic_{scheduleId}").SendAsync("ReceiveNumberUpdate", new
        {
            RoomNumber = schedule.Doctor?.RoomNumber ?? "未指定診間",
            schedule.ScheduleId,
            schedule.CurrentCallingNumber,
        });

        return schedule;
    }
}
