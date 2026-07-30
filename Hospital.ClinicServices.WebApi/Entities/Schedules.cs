using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.ClinicServices.WebApi.Entities;

//門診排班表
[Table("Schedules")]
public class Schedule
{
    [Key]
    public int ScheduleId { get; set; }

    [Required]
    public int DoctorId { get; set; }

    [ForeignKey("DoctorId")]
    virtual public Doctor? Doctor { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateTime ServiceDate { get; set; } // 看診日期

    [Required]
    public int Shift { get; set; } // 班別: 1=上午、2=下午、3=晚上

    [Required]
    public int MaxQuota { get; set; } // 最大看診人數

    [Required]
    public int CurrentRegisterCount { get; set; } // 已掛號人數

    [Required]
    public int CurrentCallingNumber { get; set; } // 目前叫號人數

    [Required]
    public int Status { get; set; } // 門診狀態 (0:未開始, 1:看診中, 2:休診, 3:已結束)
}