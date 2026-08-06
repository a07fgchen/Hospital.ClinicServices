using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.ClinicServices.WebApi.Entities;

//病例表
[Table("Patients")]
public class Patient
{
    [Key]
    public int PatientId {get; set;} //病例號碼

    [Required]
    [MaxLength(10)]
    [Column(TypeName = "varchar(10)")]
    public string NationalId {get; set;} = string.Empty; //身分證字號

    [Required]
    [MaxLength(50)]
    public string Name {get; set;} = string.Empty; //姓名

    [Required]
    [Column(TypeName = "date")]
    public DateTime BirthDate {get; set;}//出生日期

    [MaxLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string? PhoneNumber {get; set;} //電話號碼

    public bool IsFirstVisited {get; set;} //是否初診

    public DateTime CreatedAt { get; set; } = DateTime.Now; //建立時間

    public DateTime UpdatedAt { get; set; } = DateTime.Now; //更新時間
}