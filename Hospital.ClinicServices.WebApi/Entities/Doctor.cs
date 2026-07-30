using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.ClinicServices.WebApi.Entities;

[Table("Doctors")]
public class Doctor
{
    [Key]
    public int DoctorID { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }  = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Department {get; set;} = string.Empty; //科別:兒科、內科...
    
    [Required]
    [MaxLength(50)]
    public string RoomNumber {get; set;} = string.Empty; //診間號碼
}