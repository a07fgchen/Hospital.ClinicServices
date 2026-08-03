using Hospital.ClinicServices.WebApi.Entities;

namespace Hospital.ClinicServices.WebApi.Services.Interfaces;

public interface ICallingService
{
    Task<Schedule> CallingNextClinicAsync(int scheduleId);
}
