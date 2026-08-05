using Hospital.ClinicServices.WebApi.Entities;

namespace Hospital.ClinicServices.WebApi.Services.Interfaces;

public interface IScheduleService
{
    Task<List<Schedule>> GetSchedulesAsync(int depertmentId, int weekOffset, int shift = 0);
}
