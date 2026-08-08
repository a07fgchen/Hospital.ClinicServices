using Hospital.ClinicServices.WebApi.Data;
using Hospital.ClinicServices.WebApi.Entities;
using Hospital.ClinicServices.WebApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hospital.ClinicServices.WebApi.Services;

public class ScheduleService : IScheduleService
{
    private readonly ClinicDbContext _dbContext;

    public ScheduleService(ClinicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Schedule>> GetSchedulesAsync(
        int depertmentId,
        int weekOffset = 0,
        int shift = 0
        )
    {
        var today = DateTime.Today;
        if( weekOffset is not 0 )
        {
            today = today.AddDays(7);            
        }
        var diff = ((int)today.DayOfWeek + 6) % 7;

        var startOfWeek = today.AddDays(-diff); // 這週一
        var endOfWeek = startOfWeek.AddDays(7); // 這下周一

        var query = _dbContext.Schedules
            .Where(s => s.DepartmentId == depertmentId)
            .Where(s => 
                s.ServiceDate >= startOfWeek &&
                s.ServiceDate < endOfWeek
            );
        if (shift is >= 1 and <= 3)
        {
            query = query.Where(s=>s.Shift == shift);
        }

        return await query.Include(s => s.Doctor).AsNoTracking().ToListAsync();
    }
}
