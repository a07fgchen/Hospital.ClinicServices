using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.ClinicServices.WebApi.DTOs;

public class ScheduleRequestDto
{
    [FromRoute]
    public int DepertmentId { get; set; }

    [FromQuery]
    [Range(0, 1, ErrorMessage = "weekOffset必須是0或1")]
    public int WeekOffset { get; set; } = 0;

    [FromQuery]
    [Range(0, 3, ErrorMessage = "shift必須是0~3")]
    public int Shift { get; set; } = 0;
}
