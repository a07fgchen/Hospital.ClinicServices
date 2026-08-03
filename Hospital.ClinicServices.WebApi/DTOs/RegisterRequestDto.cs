using System.ComponentModel.DataAnnotations;

namespace Hospital.ClinicServices.WebApi.DTOs;

public class RegisterRequestDto
{
    //相當於Laravel的Request驗證規則
    [Required]
    public int ScheduleId  { get; set; } //排班序號

    [Required]
    public int NationalId { get; set; } //身分證

    [Required]
    public string PatientName { get; set; } = string.Empty; //病人姓名

    [Required]
    public string PhoneNumber { get; set; } = string.Empty; //病人電話
}
