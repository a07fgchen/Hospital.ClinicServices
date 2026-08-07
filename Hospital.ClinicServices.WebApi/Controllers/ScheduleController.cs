using Hospital.ClinicServices.WebApi.DTOs;
using Hospital.ClinicServices.WebApi.Entities;
using Hospital.ClinicServices.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.ClinicServices.WebApi.Controllers;

[ApiController]
[Route("api/schedule")]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet("{depertmentId:int}")]
    public async Task<IActionResult> GetSchedulesAsync(
        ScheduleRequestDto request
    )
    {
        var schedule = await _scheduleService.GetSchedulesAsync(
            request.DepertmentId,
            request.WeekOffset,
            request.Shift
        );

        return Ok(new
        {
            data = schedule
        });
    }
}