using System.ComponentModel.DataAnnotations;

namespace Hospital.ClinicServices.WebApi.DTOs;

public class FirstVisitRegisterRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "請選擇有效的門診場次。")]
    public int ScheduleId { get; set; } // 門診場次編號

    [Required(ErrorMessage = "身分證字號為必填。")]
    [RegularExpression("^[A-Z][12][0-9]{8}$", ErrorMessage = "身分證字號格式不正確。")]
    public string NationalId { get; set; } = string.Empty; //身分證

    [Required(ErrorMessage = "病患姓名為必填。")]
    [MaxLength(50)]
    public string PatientName { get; set; } = string.Empty; //病人姓名

    [Required(ErrorMessage = "電話號碼為必填。")]
    [RegularExpression("^09[0-9]{8}$", ErrorMessage = "手機號碼格式不正確。")]
    public string PhoneNumber { get; set; } = string.Empty; //病人電話

    [Required(ErrorMessage = "出生日期為必填。")]
    public DateTime BirthDate { get; set; } //病人生日
}
