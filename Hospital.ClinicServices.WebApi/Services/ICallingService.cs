using Hospital.ClinicServices.WebApi.Entities;

namespace Hospital.ClinicServices.WebApi.Services;

public interface ICallingService
{
    Task<Schedule> CallingNextClinicAsync(int scheduleId);
}
