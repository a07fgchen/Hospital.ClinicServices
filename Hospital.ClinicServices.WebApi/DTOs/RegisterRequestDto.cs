using System.ComponentModel.DataAnnotations;

namespace Hospital.ClinicServices.WebApi.DTOs;

public class RegisterRequestDto
{
    //相當於Laravel的Request驗證規則
    [Required]
    public int ScheduleId { get; set; } //排班序號

    [Required(ErrorMessage = "身分證字號為必填。")]
    [RegularExpression("^[A-Za-z][12][0-9]{8}$", ErrorMessage = "身分證字號格式不正確。")]
    public string NationalId { get; set; } = string.Empty; //身分證

    [Required(ErrorMessage = "出生日期為必填。")]
    
    public DateTime? BirthDate { get; set; } //生日
}
