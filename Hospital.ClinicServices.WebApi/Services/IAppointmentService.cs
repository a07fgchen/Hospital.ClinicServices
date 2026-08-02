using System.Threading.Tasks;
using Hospital.ClinicServices.WebApi.DTOs;
using Hospital.ClinicServices.WebApi.Entities;

namespace Hospital.ClinicServices.WebApi.Services;

public interface IAppointmentService
{
    Task<Appointment> RegisterAppointmentAsync(RegisterRequestDto request);
}
