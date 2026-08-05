using Hospital.ClinicServices.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hospital.ClinicServices.WebApi.Data;

public class ClinicDbContext : DbContext
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
    {
    }

    public DbSet<Doctor> Doctors { get; set; } = null!;

    public DbSet<Patient> Patients { get; set; } = null!;

    public DbSet<Schedule> Schedules { get; set; } = null!;

    public DbSet<Appointment> Appointments { get; set; } = null!;

    public DbSet<Department> Departments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Doctor>()
        .HasOne(d => d.Department)
        .WithMany()
        .HasForeignKey(d => d.DepartmentId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.Department)
            .WithMany()
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.Doctor)
            .WithMany()
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Schedule)
            .WithMany()
            .HasForeignKey(a => a.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // 為 NationalId 欄位建立唯一索引，確保身分證字號不會重複
        modelBuilder.Entity<Patient>()
            .HasIndex(p => p.NationalId)
            .IsUnique();


        modelBuilder.Entity<Department>().HasData(
            new Department { DepartmentId = 1, Name = "內科" },
            new Department { DepartmentId = 2, Name = "外科" },
            new Department { DepartmentId = 3, Name = "婦產科" },
            new Department { DepartmentId = 4, Name = "小兒科" },
            new Department { DepartmentId = 5, Name = "耳鼻喉科" },
            new Department { DepartmentId = 6, Name = "眼科" },
            new Department { DepartmentId = 7, Name = "皮膚科" },
            new Department { DepartmentId = 8, Name = "牙科" }
        );
    }
}