using Microsoft.AspNetCore.Mvc;

namespace Hospital.ClinicServices.WebApi.Services;

[ApiController]
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private readonly ICallingService _callingService;

    public DoctorController(ICallingService callingService)
    {
        _callingService = callingService;
    }


    //醫師叫號API
    [HttpPost("{scheduleId}/next")]
    public async Task<IActionResult> CallNext(int scheduleId)
    {
        try
        {
            var updatedSchedule = await _callingService.CallingNextClinicAsync(scheduleId);
            return Ok(new
            {
                updatedSchedule.CurrentCallingNumber,
                Message = "成功叫號下一位病人"
            });
        }
        catch (Exception exception)
        {
            return BadRequest(new { ErrorMessage = exception.Message });
        }
    }
}