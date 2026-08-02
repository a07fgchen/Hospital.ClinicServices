using System.ComponentModel.DataAnnotations;

namespace Hospital.ClinicServices.WebApi.DTOs;

public class RegisterRequestDto
{
    //相當於Laravel的Request驗證規則
    [Required]
    public int ScheduleId  { get; set; } //排班序號

    [Required]
    public int PatientId { get; set; } //病例號碼
}
