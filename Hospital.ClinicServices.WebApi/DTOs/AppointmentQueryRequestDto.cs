
using System.ComponentModel.DataAnnotations;

namespace Hospital.ClinicServices.WebApi.DTOs;

public class AppointmentQueryRequestDto
{
    [Required]
    public string NationalId { get; set; } = string.Empty;

    [Required]
    public DateTime? BirthDate { get; set; }
}