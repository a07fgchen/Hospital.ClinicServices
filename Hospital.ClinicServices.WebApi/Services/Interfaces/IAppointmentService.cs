using Hospital.ClinicServices.WebApi.DTOs;
using Hospital.ClinicServices.WebApi.Entities;

namespace Hospital.ClinicServices.WebApi.Services.Interfaces;

public interface IAppointmentService
{
    Task<Appointment> RegisterAppointmentAsync(string nationalId, int scheduleId, DateTime? birthDate);

    Task<Appointment> RegisterFirstVisitAsync(FirstVisitRegisterRequestDto request);
}
