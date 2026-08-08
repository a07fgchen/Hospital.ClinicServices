using Hospital.ClinicServices.WebApi.DTOs;
using Hospital.ClinicServices.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.ClinicServices.WebApi.Controllers;

[ApiController]
[Route("api/patient")]
public class PatientController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] PatientRegisterRequestDto request)
    {

        var patientId = await _patientService.RegisterPatientAsync(request);

        return Created(string.Empty, new
        {
            success = true,
            data = new { patientId }
        });
    }
}