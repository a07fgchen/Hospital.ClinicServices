
using System.ComponentModel.DataAnnotations;

namespace Hospital.ClinicServices.WebApi.DTOs;

public class AppointmentQueryResponseDto
{
    public int AppointmentId { get; set; }
    public int ScheduleId { get; set; }
    public int SequenceNumber { get; set; }

    public DateTime ServiceDate { get; set; }
    public int Shift { get; set; }

    public string DoctorName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;

    public bool IsToday { get; set; }

    public int? CurrentCallingNumber { get; set; }

    public int AppointmentStatus { get; set; }
}