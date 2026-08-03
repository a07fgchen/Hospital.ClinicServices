using System.ComponentModel.DataAnnotations;

namespace Hospital.ClinicServices.WebApi.DTOs;

public class PatientRegisterRequestDto
{
    [Required]
    public string NationalId { get; set; } = string.Empty; //身分證

    [Required]
    public string Name { get; set; } = string.Empty; //病人姓名

    [Required]
    public string PhoneNumber { get; set; } = string.Empty; //病人電話

    [Required]
    public DateTime BirthDate { get; set; } //病人生日
}