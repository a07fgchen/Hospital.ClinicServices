using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.ClinicServices.WebApi.Entities;

[Table("Appointments")]
public class Appointment
{
    [Key]
    public int AppointmentID {get; set;} //掛號序號

    [Required]
    public int ScheduleID {get; set;} //排班序號

    [ForeignKey("ScheduleID")]
    virtual public Schedule? Schedule {get; set;}

    [Required]
    public int PatientID {get; set;} //病例號碼

    [ForeignKey("PatientID")]
    virtual public Patient? Patient {get; set;}
    
    [Required]
    public int SequenceNumber  {get; set;} //看診號碼

    [Required]
    public int AppointmentStatus {get; set;} //掛號狀態 (0:已掛號, 1:已取消, 2:已看診)

    [Required]
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow; //掛號日期
}