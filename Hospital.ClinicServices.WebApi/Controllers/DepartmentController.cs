using Hospital.ClinicServices.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.ClinicServices.WebApi.Controllers;

[ApiController]
[Route("api/department")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDepartments()
    {
        try
        {
            var departments = await _departmentService.GetDepartmentsAsync();
            return Ok(new
            {
                success = true,
                data = departments
            });
        }
        catch (Exception exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "取得科別列表失敗",
                Detail = exception.Message
            });
            throw;
        }
    }
}