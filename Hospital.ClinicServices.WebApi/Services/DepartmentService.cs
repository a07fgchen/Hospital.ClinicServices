using Hospital.ClinicServices.WebApi.Data;
using Hospital.ClinicServices.WebApi.Entities;
using Hospital.ClinicServices.WebApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hospital.ClinicServices.WebApi.Services;

public class DepartmentService : IDepartmentService
{
    private readonly ClinicDbContext _dbContext;

    public DepartmentService(ClinicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Department>> GetDepartmentsAsync()
    {
        var department = await _dbContext.Departments.ToListAsync();
        return department;
    }
}