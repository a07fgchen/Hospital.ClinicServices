using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.ClinicServices.WebApi.Entities;

//醫生表
[Table("Doctors")]
public class Doctor
{
    [Key]
    public int DoctorId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }  = string.Empty;

    [Required]
    public int DepartmentId {get; set;} = 0;

    [ForeignKey("DepartmentId")]
    public Department? Department { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string RoomNumber {get; set;} = string.Empty; //診間號碼
}