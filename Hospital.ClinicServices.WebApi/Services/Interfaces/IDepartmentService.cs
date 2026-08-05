using Hospital.ClinicServices.WebApi.Entities;

namespace Hospital.ClinicServices.WebApi.Services.Interfaces;

public interface IDepartmentService
{
    Task<List<Department>> GetDepartmentsAsync();
}
