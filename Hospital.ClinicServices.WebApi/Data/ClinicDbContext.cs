using Hospital.ClinicServices.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hospital.ClinicServices.WebApi.Data;

public class ClinicDbContext : DbContext
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext>options): base(options)
    {
    }

    public DbSet<Doctor> Doctors { get; set; } = null!;

    public DbSet<Patient> Patients { get; set; } = null!;

    public DbSet<Schedule> Schedules { get; set; } = null!;

    public DbSet<Appointment> Appointments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 為 NationalId 欄位建立唯一索引，確保身分證字號不會重複
        modelBuilder.Entity<Patient>()
            .HasIndex(p => p.NationalId)
            .IsUnique();

        modelBuilder.Entity<Schedule>()
            .HasIndex(s => new { s.ServiceDate, s.Shift });

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.ScheduleId, a.SequenceNumber})
            .IsUnique();
    }
}