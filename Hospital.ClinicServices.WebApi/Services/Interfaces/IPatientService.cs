using Hospital.ClinicServices.WebApi.DTOs;

namespace Hospital.ClinicServices.WebApi.Services.Interfaces;

public interface IPatientService
{
    Task<int> RegisterPatientAsync(PatientRegisterRequestDto request);
}