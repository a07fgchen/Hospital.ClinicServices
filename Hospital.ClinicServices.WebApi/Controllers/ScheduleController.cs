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
        [FromRoute] int depertmentId,
        [FromQuery] int weekOffset = 0,
        [FromQuery] int shift = 0
    )
    {
        try
        {
            var schedule = await _scheduleService.GetSchedulesAsync(depertmentId, weekOffset, shift);

            return Ok(new
            {
                data = schedule
            });
        }
        catch (Exception exception)
        {

            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }


}