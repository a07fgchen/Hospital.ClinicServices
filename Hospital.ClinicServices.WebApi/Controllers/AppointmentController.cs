using Hospital.ClinicServices.WebApi.DTOs;
using Hospital.ClinicServices.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.ClinicServices.WebApi.Controllers;

[ApiController]
[Route("api/appointment")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }
    //民眾初診掛號API
    [HttpPost("register-first-visit")]
    public async Task<IActionResult> RegisterFirstVisitAsync([FromBody] FirstVisitRegisterRequestDto request)
    {

        var result = await _appointmentService.RegisterFirstVisitAsync(request);

        return CreateRegistrationResponse(result);

    }

    //民眾複診掛號API 
    [HttpPost("register")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {

        var result = await _appointmentService.RegisterAppointmentAsync(
            request.NationalId,
            request.ScheduleId,
            request.BirthDate
        );
        return CreateRegistrationResponse(result);
    }

    [HttpPost("query")]
    public async Task<IActionResult> QueryAppointments(
        AppointmentQueryRequestDto request)
    {
        var appointmets = await _appointmentService.QueryAppointmentsAsync(
            request.NationalId,
            request.BirthDate
        );

        return Ok(new
        {
            data = appointmets
        });
    }
    private CreatedResult CreateRegistrationResponse(Entities.Appointment result)
    {
        return Created(string.Empty, new
        {
            success = true,
            data = new
            {
                appointmentId = result.AppointmentId,
                sequenceNumber = result.SequenceNumber,
                createdAt = result.CreatedAt,
                message = "掛號成功",
                status = "已預約"
            }
        });
    }
}
