using Hospital.ClinicServices.WebApi.Data;
using Hospital.ClinicServices.WebApi.DTOs;
using Hospital.ClinicServices.WebApi.Entities;
using Hospital.ClinicServices.WebApi.Services.Interfaces;

public class PatientService : IPatientService
{
    private readonly ClinicDbContext _dbContext;

    public PatientService(ClinicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> RegisterPatientAsync(PatientRegisterRequestDto request)
    {
        var patient = new Patient
        {
            Name = request.Name,
            NationalId = request.NationalId,
            BirthDate = request.BirthDate,
            PhoneNumber = request.PhoneNumber
        };

        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        return patient.PatientId;
    }

}